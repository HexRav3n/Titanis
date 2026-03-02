using ms_drsr;
using ms_dtyp;
using System;
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

			ms_dtyp.GUID clientDsaGuid = clientGuid.ToRpcGuid();
			var puuidClientDsa = new RpcPointer<ms_dtyp.GUID>(clientDsaGuid);

			// Some DC/build combinations are strict about the DRS_EXTENSIONS_INT size.
			// Try modern profile first, then progressively smaller legacy profiles.
			var extBlobs = new[]
			{
				BuildDrsExtensionsBlob(extFlagsUint, includeExtCaps: true, extCaps: 0xFFFFFFFF),
				BuildDrsExtensionsBlob(extFlagsUint, includeExtCaps: true, extCaps: 0),
				BuildDrsExtensionsBlob(extFlagsUint, includeExtCaps: false, extCaps: 0),
			};

			Win32ErrorCode lastError = 0;
			foreach (var extBlob in extBlobs)
			{
				var pextClient = new RpcPointer<DRS_EXTENSIONS>(new DRS_EXTENSIONS { cb = (uint)extBlob.Length, rgb = extBlob });
				var ppextServer = new RpcPointer<RpcPointer<DRS_EXTENSIONS>>(new RpcPointer<DRS_EXTENSIONS>());
				var phDrs = new RpcPointer<RpcContextHandle>();

				var ret = await this._proxy.IDL_DRSBind(
					puuidClientDsa,
					pextClient,
					ppextServer,
					phDrs,
					cancellationToken).ConfigureAwait(false);

				lastError = (Win32ErrorCode)ret;
				if (lastError == 0)
					return new DrsDsa(this, phDrs.value);
				if (lastError != Win32ErrorCode.RPC_X_BAD_STUB_DATA)
					lastError.CheckAndThrow();
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

		private static void WriteUInt32Le(byte[] buffer, int offset, uint value)
		{
			buffer[offset] = (byte)value;
			buffer[offset + 1] = (byte)(value >> 8);
			buffer[offset + 2] = (byte)(value >> 16);
			buffer[offset + 3] = (byte)(value >> 24);
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
