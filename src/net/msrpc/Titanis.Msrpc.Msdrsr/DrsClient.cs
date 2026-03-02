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
		/// Well-known named pipe name for the DRSR interface.
		/// </summary>
		public const string PipeName = "drsuapi";

		#region Connection parameters
		// [MS-DRSR] § 2.1
		/// <inheritdoc/>
		public sealed override string? WellKnownPipeName => PipeName;
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
			// Advertise capabilities to the DC. We want V6 replies and strong encryption.
			var extFlags =
				DRS_EXTENSIONS_IN_FLAGS.DRS_EXT_BASE |
				DRS_EXTENSIONS_IN_FLAGS.DRS_EXT_GETCHG_DEFLATE |
				DRS_EXTENSIONS_IN_FLAGS.DRS_EXT_GETCHGREPLY_V6 |
				DRS_EXTENSIONS_IN_FLAGS.DRS_EXT_STRONG_ENCRYPTION |
				DRS_EXTENSIONS_IN_FLAGS.DRS_EXT_GETCHGREQ_V8 |
				DRS_EXTENSIONS_IN_FLAGS.DRS_EXT_LINKED_VALUE_REPLICATION;

			// Build the extensions blob (4-byte flags LE)
			byte[] extBlob = new byte[4];
			uint extFlagsUint = (uint)extFlags;
			extBlob[0] = (byte)(extFlagsUint);
			extBlob[1] = (byte)(extFlagsUint >> 8);
			extBlob[2] = (byte)(extFlagsUint >> 16);
			extBlob[3] = (byte)(extFlagsUint >> 24);

			var pextClient = new RpcPointer<DRS_EXTENSIONS>(new DRS_EXTENSIONS { rgb = extBlob });
			var ppextServer = new RpcPointer<RpcPointer<DRS_EXTENSIONS>>(new RpcPointer<DRS_EXTENSIONS>());
			var phDrs = new RpcPointer<RpcContextHandle>();

			ms_dtyp.GUID clientDsaGuid = clientGuid.ToRpcGuid();
			var puuidClientDsa = new RpcPointer<ms_dtyp.GUID>(clientDsaGuid);

			var ret = await this._proxy.IDL_DRSBind(
				puuidClientDsa,
				pextClient,
				ppextServer,
				phDrs,
				cancellationToken).ConfigureAwait(false);

			((Win32ErrorCode)ret).CheckAndThrow();

			return new DrsDsa(this, phDrs.value);
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
