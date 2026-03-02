using ms_drsr;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Titanis.DceRpc;

namespace Titanis.Msrpc.Msdrsr
{
	/// <summary>
	/// Represents an authenticated DRS session (DSA context handle).
	/// Wraps IDL_DRSBind / IDL_DRSUnbind lifecycle.
	/// </summary>
	public class DrsDsa : DrsObject
	{
		// [MS-DRSR] § 4.1.10: Win32 error indicating more data is available
		private const uint ERROR_MORE_DATA = 0x000000EA;

		internal DrsDsa(DrsClient client, RpcContextHandle handle)
			: base(client, handle)
		{
		}

		protected override Task CloseAsync(CancellationToken cancellationToken)
			=> this._client.UnbindAsync(this._handle, cancellationToken);

		/// <summary>
		/// Retrieves all NC changes for the given naming context, automatically
		/// paginating until no more data is available.
		/// </summary>
		/// <param name="namingContextDn">Distinguished name of the naming context (e.g. "DC=corp,DC=local")</param>
		/// <param name="flags">DRS_OPTIONS flags (e.g. DRS_INIT_SYNC | DRS_WRIT_REP)</param>
		/// <param name="partialAttrs">Attribute type IDs to replicate; null for all secret attributes</param>
		/// <param name="objectGuid">GUID of a specific object to replicate; empty GUID for entire NC</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>All replicated entry infos from this NC</returns>
		public async Task<List<REPLENTINFLIST>> GetNCChangesAllAsync(
			string namingContextDn,
			DRS_OPTIONS flags,
			uint[]? partialAttrs,
			Guid objectGuid,
			CancellationToken cancellationToken)
		{
			var results = new List<REPLENTINFLIST>();
			USN_VECTOR usnFrom = new USN_VECTOR();
			bool moreData;
			int pageCount = 0;

			do
			{
				pageCount++;
				if (pageCount > 1024)
					throw new InvalidOperationException("DRSGetNCChanges exceeded 1024 pages without completing.");

				var req = BuildGetNCChangesRequest(namingContextDn, flags, partialAttrs, objectGuid, usnFrom);
				DRS_MSG_GETCHGREPLY_V6 reply = await this._client.GetNCChangesAsync(
					this._handle, req, cancellationToken).ConfigureAwait(false);

				// Walk the linked list and collect all entries
				var node = reply.pObjects;
				while (node != null)
				{
					results.Add(node.value);
					node = node.value.pNextEntInf;
				}

				moreData = reply.fMoreData;
				if (moreData)
				{
					if (IsUsnVectorEqual(reply.usnvecTo, usnFrom))
					{
						throw new InvalidOperationException(
							"DRSGetNCChanges signaled more data but replication cursor did not advance.");
					}
					// Next page starts where this one left off
					usnFrom = reply.usnvecTo;
				}
			}
			while (moreData);

			return results;
		}

		/// <summary>
		/// Retrieves all NC changes and yields them along with the prefix table
		/// from each reply for attribute decoding.
		/// </summary>
		public async Task<(List<REPLENTINFLIST> Entries, SCHEMA_PREFIX_TABLE PrefixTable)> GetNCChangesWithPrefixTableAsync(
			string namingContextDn,
			DRS_OPTIONS flags,
			uint[]? partialAttrs,
			Guid objectGuid,
			CancellationToken cancellationToken)
		{
			var results = new List<REPLENTINFLIST>();
			SCHEMA_PREFIX_TABLE lastPrefixTable = default;
			USN_VECTOR usnFrom = new USN_VECTOR();
			bool moreData;
			int pageCount = 0;

			do
			{
				pageCount++;
				if (pageCount > 1024)
					throw new InvalidOperationException("DRSGetNCChanges exceeded 1024 pages without completing.");

				var req = BuildGetNCChangesRequest(namingContextDn, flags, partialAttrs, objectGuid, usnFrom);
				DRS_MSG_GETCHGREPLY_V6 reply = await this._client.GetNCChangesAsync(
					this._handle, req, cancellationToken).ConfigureAwait(false);

				lastPrefixTable = reply.PrefixTableSrc;

				var node = reply.pObjects;
				while (node != null)
				{
					results.Add(node.value);
					node = node.value.pNextEntInf;
				}

				moreData = reply.fMoreData;
				if (moreData)
				{
					if (IsUsnVectorEqual(reply.usnvecTo, usnFrom))
					{
						throw new InvalidOperationException(
							"DRSGetNCChanges signaled more data but replication cursor did not advance.");
					}
					usnFrom = reply.usnvecTo;
				}
			}
			while (moreData);

			return (results, lastPrefixTable);
		}

		private static bool IsUsnVectorEqual(USN_VECTOR a, USN_VECTOR b)
		{
			return a.usnHighObjUpdate == b.usnHighObjUpdate
				&& a.usnReserved == b.usnReserved
				&& a.usnHighPropUpdate == b.usnHighPropUpdate;
		}

		/// <summary>
		/// Translates object names between formats using IDL_DRSCrackNames.
		/// </summary>
		public Task<DS_NAME_RESULTW> CrackNamesAsync(
			string[] names,
			DS_NAME_FORMAT formatOffered,
			DS_NAME_FORMAT formatDesired,
			DS_NAME_FLAGS flags,
			CancellationToken cancellationToken)
			=> this._client.CrackNamesAsync(this._handle, names, formatOffered, formatDesired, flags, cancellationToken);

		private static DRS_MSG_GETCHGREQ_V8 BuildGetNCChangesRequest(
			string namingContextDn,
			DRS_OPTIONS flags,
			uint[]? partialAttrs,
			Guid objectGuid,
			USN_VECTOR usnFrom)
		{
			// Build the NC DSNAME
			char[] nameChars = (namingContextDn + '\0').ToCharArray();
			// DSNAME.structLen should include the NDR conformant header + fixed body + WCHAR data.
			// This mirrors known-good client behavior and avoids server-side stub correlation failures.
			uint structLen = (uint)(60 + (nameChars.Length * sizeof(char)));
			var pNC = new RpcPointer<DSNAME>(new DSNAME
			{
				structLen = structLen,
				SidLen = 0,
				Guid = objectGuid.ToRpcGuid(),
				Sid = new byte[28],
				NameLen = (uint)nameChars.Length,
				StringName = nameChars,
			});

			RpcPointer<PARTIAL_ATTR_VECTOR_V1_EXT>? pPartialAttrSet = null;
			if (partialAttrs != null && partialAttrs.Length > 0)
			{
				pPartialAttrSet = new RpcPointer<PARTIAL_ATTR_VECTOR_V1_EXT>(new PARTIAL_ATTR_VECTOR_V1_EXT
				{
					dwVersion = 1,
					dwReserved1 = 0,
					cAttrs = (uint)partialAttrs.Length,
					rgPartialAttr = partialAttrs,
				});
			}

			return new DRS_MSG_GETCHGREQ_V8
			{
				uuidDsaObjDest = new ms_dtyp.GUID(),  // zeroed — let server fill
				uuidInvocIdSrc = new ms_dtyp.GUID(),
				pNC = pNC,
				usnvecFrom = usnFrom,
				pUpToDateVecDest = null,
				ulFlags = (uint)flags,
				cMaxObjects = 1000,
				cMaxBytes = 0x00A00000,  // 10 MB
				ulExtendedOp = 0,
				liFsmoInfo = 0,
				pPartialAttrSet = pPartialAttrSet,
				pPartialAttrSetEx = null,
				PrefixTableDest = new SCHEMA_PREFIX_TABLE(),
			};
		}
	}
}
