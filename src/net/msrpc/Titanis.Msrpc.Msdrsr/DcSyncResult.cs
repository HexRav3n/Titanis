namespace Titanis.Msrpc.Msdrsr
{
	/// <summary>
	/// Credentials extracted from a domain controller via DCSync (DRSGetNCChanges).
	/// </summary>
	public class DcSyncResult
	{
		/// <summary>SAM account name (e.g. "krbtgt")</summary>
		public string? SamAccountName { get; set; }

		/// <summary>Distinguished name of the account object</summary>
		public string? DistinguishedName { get; set; }

		/// <summary>Security identifier (S-1-5-...)</summary>
		public string? Sid { get; set; }

		/// <summary>NT hash (MD4 of Unicode password), hex-encoded (32 chars)</summary>
		public string? NtHash { get; set; }

		/// <summary>LM hash, hex-encoded (32 chars), or null if not present</summary>
		public string? LmHash { get; set; }

		/// <summary>AES-256 Kerberos key, hex-encoded, or null if not present</summary>
		public string? Aes256Key { get; set; }

		/// <summary>AES-128 Kerberos key, hex-encoded, or null if not present</summary>
		public string? Aes128Key { get; set; }

		/// <summary>DES-CBC-MD5 Kerberos key, hex-encoded, or null if not present</summary>
		public string? DesKey { get; set; }
	}
}
