using ms_drsr;
using ms_dtyp;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Titanis.DceRpc;
using Titanis.DceRpc.Client;
using Titanis.Security;
using Titanis.Winterop;

namespace Titanis.Msrpc.Msdrsr
{
	/// <summary>
	/// Implements a client for [MS-DRSR] (Directory Replication Service Remote Protocol).
	/// </summary>
	public class DrsClient : RpcServiceClient<ms_drsr.drsClientProxy>
	{
		/// <summary>
		/// Optional sink for verbose DRS bind diagnostics.
		/// </summary>
		public Action<string>? DiagnosticSink { get; set; }

		/// <summary>
		/// Default client GUID used by native NTDS API callers.
		/// </summary>
		public static readonly Guid NtdsApiClientGuid = new("e24d201a-4fd6-11d1-a3da-0000f875ae0d");

		#region Connection parameters
		// MS-DRSR should be resolved via endpoint mapper over TCP.
		// Avoid SMB named pipe fallback for this client.
		/// <inheritdoc/>
		public sealed override string? WellKnownPipeName => null;
		// [MS-DRSR] § 2.1
		/// <inheritdoc/>
		public sealed override bool SupportsDynamicTcp => true;
		// drsuapi does not negotiate NDR64
		/// <inheritdoc/>
		public sealed override bool SupportsNdr64 => false;
		/// <inheritdoc/>
		public sealed override string? ServiceClass => ServiceClassNames.HostU;
		// [MS-DRSR] § 2.1: packet-level privacy (sealing) required
		/// <inheritdoc/>
		public sealed override bool RequiresEncryptionOverTcp => true;
		#endregion

		/// <summary>
		/// Binds to the Directory Replication Service on the DC.
		/// Returns a <see cref="DrsDsa"/> context for subsequent operations.
		/// </summary>
		/// <param name="clientGuid">Client DSA GUID (may be all zeros for anonymous)</param>
		/// <param name="cancellationToken">Cancellation token</param>
		public async Task<DrsDsa> BindAsync(Guid clientGuid, CancellationToken cancellationToken)
		{
			// Match known-good DRSUAPI clients (Impacket/Mimikatz behavior):
			// DRS_EXT_GETCHGREQ_V6 | DRS_EXT_GETCHGREPLY_V6 |
			// DRS_EXT_GETCHGREQ_V8 | DRS_EXT_STRONG_ENCRYPTION
			const uint extFlagsUint = 0x05408000;

			if (clientGuid == Guid.Empty)
				clientGuid = NtdsApiClientGuid;
			this.EmitDiagnostic(
				$"[SuperDiag] DRSBind start clientGuid={clientGuid} extFlags=0x{extFlagsUint:X8}");

			ms_dtyp.GUID clientDsaGuid = clientGuid.ToRpcGuid();
			var puuidClientDsa = new RpcPointer<ms_dtyp.GUID>(clientDsaGuid);

			// Some DC/build combinations are strict about DRSBind extension blob layout.
			// Try several known profiles.
			var extProfiles = new (string Name, byte[] Blob)[]
			{
				("52-extcaps-ffffffff", BuildDrsExtensionsBlob(extFlagsUint, includeExtCaps: true, extCaps: 0xFFFFFFFF)),
				("52-extcaps-00000000", BuildDrsExtensionsBlob(extFlagsUint, includeExtCaps: true, extCaps: 0)),
				("48-no-extcaps", BuildDrsExtensionsBlob(extFlagsUint, includeExtCaps: false, extCaps: 0)),
				("56-inner-cb-extcaps-ffffffff", BuildDrsExtensionsBlobWithInnerCb(extFlagsUint, includeExtCaps: true, extCaps: 0xFFFFFFFF)),
				("8-legacy-short", BuildLegacyShortExtensionsBlob(extFlagsUint)),
			};

			Win32ErrorCode lastError = 0;
			for (int i = 0; i < extProfiles.Length; i++)
			{
				var profile = extProfiles[i];
				var extBlob = profile.Blob;
				var pextClient = new RpcPointer<DRS_EXTENSIONS>(new DRS_EXTENSIONS { cb = (uint)extBlob.Length, rgb = extBlob });
				var ppextServer = new RpcPointer<RpcPointer<DRS_EXTENSIONS>>(new RpcPointer<DRS_EXTENSIONS>());
				var phDrs = new RpcPointer<RpcContextHandle>();
				this.EmitDiagnostic(
					$"[SuperDiag] DRSBind attempt {i + 1}/{extProfiles.Length} profile={profile.Name} cb={extBlob.Length} rgbHex={FormatHex(extBlob)}");

				try
				{
					var ret = await this._proxy.IDL_DRSBind(
						puuidClientDsa,
						pextClient,
						ppextServer,
						phDrs,
						cancellationToken).ConfigureAwait(false);

					lastError = (Win32ErrorCode)ret;
					this.EmitDiagnostic(
						$"[SuperDiag] DRSBind attempt {i + 1} profile={profile.Name} returned 0x{(uint)lastError:X8}");
					if (lastError == 0)
					{
						this.EmitDiagnostic(
							$"[SuperDiag] DRSBind success profile={profile.Name} ctx={phDrs.value.contextId}");
						return new DrsDsa(this, phDrs.value);
					}
					if (lastError != Win32ErrorCode.RPC_X_BAD_STUB_DATA)
						lastError.CheckAndThrow();
				}
				catch (Win32Exception ex) when ((uint)ex.NativeErrorCode == (uint)Win32ErrorCode.RPC_X_BAD_STUB_DATA)
				{
					lastError = Win32ErrorCode.RPC_X_BAD_STUB_DATA;
					this.EmitDiagnostic(
						$"[SuperDiag] DRSBind attempt {i + 1} profile={profile.Name} threw RPC_X_BAD_STUB_DATA ({ex.Message})");
				}
			}

			if (lastError == Win32ErrorCode.RPC_X_BAD_STUB_DATA)
			{
				throw new InvalidOperationException(
					"DRSBind failed with RPC_X_BAD_STUB_DATA for all extension blob profiles (52/extcaps=ffffffff, 52/extcaps=0, 48/no-extcaps, 56/inner-cb, 8/legacy).");
			}

			lastError.CheckAndThrow();
			throw new InvalidOperationException("Unreachable.");
		}

		private static byte[] BuildDrsExtensionsBlob(uint dwFlags, bool includeExtCaps, uint extCaps)
		{
			int cb = includeExtCaps ? 52 : 48;
			byte[] extBlob = new byte[cb];
			WriteUInt32Le(extBlob, 0, dwFlags); // dwFlags
			// SiteObjGuid is all zeros (offset 4..19)
			WriteUInt32Le(extBlob, 20, 0);      // Pid
			WriteUInt32Le(extBlob, 24, 0);      // dwReplEpoch
			WriteUInt32Le(extBlob, 28, 0);      // dwFlagsExt
			// ConfigObjGUID is all zeros (offset 32..47)
			if (includeExtCaps)
				WriteUInt32Le(extBlob, 48, extCaps); // dwExtCaps
			return extBlob;
		}

		private static byte[] BuildDrsExtensionsBlobWithInnerCb(uint dwFlags, bool includeExtCaps, uint extCaps)
		{
			int cb = includeExtCaps ? 56 : 52;
			byte[] extBlob = new byte[cb];
			WriteUInt32Le(extBlob, 0, (uint)cb); // inner cb
			WriteUInt32Le(extBlob, 4, dwFlags);  // dwFlags
			// SiteObjGuid zeros (offset 8..23)
			WriteUInt32Le(extBlob, 24, 0);       // Pid
			WriteUInt32Le(extBlob, 28, 0);       // dwReplEpoch
			WriteUInt32Le(extBlob, 32, 0);       // dwFlagsExt
			// ConfigObjGUID zeros (offset 36..51)
			if (includeExtCaps)
				WriteUInt32Le(extBlob, 52, extCaps); // dwExtCaps
			return extBlob;
		}

		private static byte[] BuildLegacyShortExtensionsBlob(uint dwFlags)
		{
			byte[] extBlob = new byte[8];
			WriteUInt32Le(extBlob, 0, 8);       // inner cb
			WriteUInt32Le(extBlob, 4, dwFlags); // dwFlags
			return extBlob;
		}

		private static void WriteUInt32Le(byte[] buffer, int offset, uint value)
		{
			buffer[offset] = (byte)value;
			buffer[offset + 1] = (byte)(value >> 8);
			buffer[offset + 2] = (byte)(value >> 16);
			buffer[offset + 3] = (byte)(value >> 24);
		}

		private void EmitDiagnostic(string message)
		{
			this.DiagnosticSink?.Invoke(message);
		}

		private static string FormatHex(byte[] bytes)
		{
			const int maxBytes = 64;
			int take = Math.Min(maxBytes, bytes.Length);
			string hex = Convert.ToHexString(bytes, 0, take).ToLowerInvariant();
			return (bytes.Length > maxBytes) ? $"{hex}..." : hex;
		}

		internal async Task UnbindAsync(RpcContextHandle handle, CancellationToken cancellationToken)
		{
			var phDrs = new RpcPointer<RpcContextHandle>(handle);
			await this._proxy.IDL_DRSUnbind(phDrs, cancellationToken).ConfigureAwait(false);
		}

		internal async Task<DRS_MSG_GETCHGREPLY_V6> GetNCChangesAsync(
			RpcContextHandle handle,
			DRS_MSG_GETCHGREQ_V8 request,
			CancellationToken cancellationToken)
		{
			var pdwOutVersion = new RpcPointer<uint>();
			var pmsgOut = new RpcPointer<DRS_MSG_GETCHGREPLY_V6>();

			var ret = await this._proxy.IDL_DRSGetNCChanges(
				handle,
				request,
				pdwOutVersion,
				pmsgOut,
				cancellationToken).ConfigureAwait(false);

			((Win32ErrorCode)ret).CheckAndThrow();

			return pmsgOut.value;
		}

		internal async Task<DS_NAME_RESULTW> CrackNamesAsync(
			RpcContextHandle handle,
			string[] names,
			DS_NAME_FORMAT formatOffered,
			DS_NAME_FORMAT formatDesired,
			DS_NAME_FLAGS flags,
			CancellationToken cancellationToken)
		{
			var request = new DRS_MSG_CRACKREQ_V1
			{
				CodePage = 0,
				LocaleId = 0,
				dwFlags = (uint)flags,
				formatOffered = (uint)formatOffered,
				formatDesired = (uint)formatDesired,
				cNames = (uint)names.Length,
				rpNames = new RpcPointer<string[]>(names),
			};

			var pdwOutVersion = new RpcPointer<uint>();
			var pmsgOut = new RpcPointer<DRS_MSG_CRACKREPLY_V1>();

			var ret = await this._proxy.IDL_DRSCrackNames(
				handle,
				request,
				pdwOutVersion,
				pmsgOut,
				cancellationToken).ConfigureAwait(false);

			((Win32ErrorCode)ret).CheckAndThrow();

			return pmsgOut.value.pResult?.value ?? new DS_NAME_RESULTW();
		}

		/// <summary>
		/// Returns the RPC session key for decrypting replicated secrets.
		/// Requires that the channel was authenticated with packet-level privacy.
		/// </summary>
		public byte[] GetSessionKey()
		{
			if (this._proxy.SecureChannel is null || !this._proxy.SecureChannel.HasSessionKey)
				throw new InvalidOperationException(
					"No RPC session key available. MS-DRSR requires PacketPrivacy authentication.");

			return this._proxy.SecureChannel.GetSessionKey()
				?? throw new InvalidOperationException("RPC session key is null.");
		}
	}
}
