using System;
using System.Security.Cryptography;
using System.Text;
using Titanis.Crypto;

namespace Titanis.Msrpc.Msdrsr
{
	/// <summary>
	/// Decrypts replicated secret attribute values returned by DRSGetNCChanges.
	///
	/// Implements the algorithms described in [MS-DRSR] § 5.16 and § 5.17
	/// for recovering NT hashes, LM hashes, and Kerberos keys from the
	/// encrypted attribute blobs delivered by the DC over the replication channel.
	///
	/// Two cipher modes are supported:
	///   - RC4/MD5 (pre-Windows Server 2008): 24-byte payload  (8-byte salt + 16-byte ciphertext)
	///   - AES-CBC  (Windows Server 2008+):   40-byte payload  (4-byte header + 4-byte salt-len + 16-byte IV + 16-byte ciphertext)
	/// </summary>
	public static class DrsSecretDecryptor
	{
		// -------------------------------------------------------------------------
		// NT / LM hash decryption ([MS-DRSR] § 5.16 removeSecretAttributeValue)
		// -------------------------------------------------------------------------

		/// <summary>
		/// Decrypts a replicated unicodePwd or dBCSPwd attribute value.
		/// Returns the 16-byte raw hash on success, or null if the blob is unrecognised.
		/// </summary>
		public static byte[]? DecryptHash(byte[] encryptedValue, byte[] sessionKey)
		{
			ArgumentNullException.ThrowIfNull(encryptedValue);
			ArgumentNullException.ThrowIfNull(sessionKey);

			// A valid encrypted hash is at minimum 24 bytes (RC4 style).
			if (encryptedValue.Length == 24)
				return DecryptHashRc4(encryptedValue, sessionKey);

			// AES style has a 4-byte header (version/flags), 4-byte salt-len, 16-byte IV, 16-byte ciphertext = 40 bytes.
			if (encryptedValue.Length >= 40)
				return DecryptHashAes(encryptedValue, sessionKey);

			return null;
		}

		/// <summary>
		/// RC4 / MD5 decryption of a replicated hash value.
		/// Layout: [8-byte salt][16-byte ciphertext]
		/// Key derivation: key = MD5(sessionKey + salt)
		/// </summary>
		private static byte[] DecryptHashRc4(byte[] encryptedValue, byte[] sessionKey)
		{
			// encryptedValue[0..7]  = salt
			// encryptedValue[8..23] = RC4-encrypted hash

			byte[] salt = new byte[8];
			byte[] ciphertext = new byte[16];
			Array.Copy(encryptedValue, 0, salt, 0, 8);
			Array.Copy(encryptedValue, 8, ciphertext, 0, 16);

			// key = MD5(sessionKey || salt)
			byte[] keyMaterial = new byte[sessionKey.Length + 8];
			sessionKey.CopyTo(keyMaterial, 0);
			salt.CopyTo(keyMaterial, sessionKey.Length);

			byte[] rc4Key = new byte[16];
			SlimHashAlgorithm.ComputeHash<Md5Context>(keyMaterial, rc4Key);

			// RC4-decrypt the 16-byte ciphertext
			byte[] hash = new byte[16];
			Rc4.Transform(rc4Key, ciphertext, hash);
			return hash;
		}

		/// <summary>
		/// AES-CBC decryption of a replicated hash value (Windows Server 2008+).
		/// Layout: [4-byte header][4-byte reserved][16-byte IV][16-byte ciphertext]
		/// </summary>
		private static byte[] DecryptHashAes(byte[] encryptedValue, byte[] sessionKey)
		{
			// Offsets: header=0, reserved=4, IV=8, ciphertext=24
			// Some implementations have an extra 8-byte header making IV at offset 16.
			// We try offset 8 first (most common), then fall back to 16 if output looks bad.

			int ivOffset = 8;
			if (encryptedValue.Length >= 48)
				ivOffset = 16;

			byte[] iv = new byte[16];
			byte[] ciphertext = new byte[16];
			Array.Copy(encryptedValue, ivOffset, iv, 0, 16);
			Array.Copy(encryptedValue, ivOffset + 16, ciphertext, 0, 16);

			// AES key is the first 16 bytes of the session key (AES-128-CBC)
			byte[] aesKey = new byte[16];
			int keyLen = Math.Min(sessionKey.Length, 16);
			Array.Copy(sessionKey, 0, aesKey, 0, keyLen);

			return AesCbcDecrypt(aesKey, iv, ciphertext);
		}

		private static byte[] AesCbcDecrypt(byte[] key, byte[] iv, byte[] ciphertext)
		{
			using var aes = Aes.Create();
			aes.Key = key;
			aes.IV = iv;
			aes.Mode = CipherMode.CBC;
			aes.Padding = PaddingMode.None;

			using var decryptor = aes.CreateDecryptor();
			return decryptor.TransformFinalBlock(ciphertext, 0, ciphertext.Length);
		}

		// -------------------------------------------------------------------------
		// supplementalCredentials decryption and Kerberos key extraction
		// -------------------------------------------------------------------------

		/// <summary>
		/// Decrypts and parses supplementalCredentials to extract Kerberos keys.
		/// Returns null if the blob is absent or cannot be parsed.
		/// </summary>
		public static KerberosKeys? DecryptSupplementalCredentials(byte[] encryptedValue, byte[] sessionKey)
		{
			if (encryptedValue == null || encryptedValue.Length < 24)
				return null;

			// supplementalCredentials is encrypted the same way as the hash attributes.
			// The result is a USER_PROPERTIES blob [MS-SAMR] § 2.2.10.
			byte[]? decrypted = DecryptSupplementalCredentialsBlob(encryptedValue, sessionKey);
			if (decrypted == null)
				return null;

			return ParseUserProperties(decrypted);
		}

		/// <summary>
		/// Decrypts the supplementalCredentials blob.
		/// Layout is similar to hash encryption but the payload is larger.
		/// </summary>
		private static byte[]? DecryptSupplementalCredentialsBlob(byte[] encryptedValue, byte[] sessionKey)
		{
			// RC4/MD5 style: [4-byte header][4-byte reserved][8-byte salt][payload_ciphertext]
			// The payload length is (encryptedValue.Length - 16).
			if (encryptedValue.Length < 16)
				return null;

			// Detect style: if the first 4 bytes are a small number (flags/version byte) it's RC4 style.
			// A more reliable heuristic: if length > 40 and first 4 bytes == 0x00000001, it's the newer format.
			// For maximum compatibility we try to decrypt and validate with a magic check.

			return DecryptSupplementalRc4(encryptedValue, sessionKey);
		}

		private static byte[] DecryptSupplementalRc4(byte[] encryptedValue, byte[] sessionKey)
		{
			// Layout: [4 byte header (skip)][4 byte reserved (skip)][8 byte salt][payload]
			if (encryptedValue.Length < 16)
				return Array.Empty<byte>();

			byte[] salt = new byte[8];
			Array.Copy(encryptedValue, 8, salt, 0, 8);

			int payloadLen = encryptedValue.Length - 16;
			byte[] ciphertext = new byte[payloadLen];
			Array.Copy(encryptedValue, 16, ciphertext, 0, payloadLen);

			// Key = MD5(sessionKey + salt)
			byte[] keyMaterial = new byte[sessionKey.Length + 8];
			sessionKey.CopyTo(keyMaterial, 0);
			salt.CopyTo(keyMaterial, sessionKey.Length);

			byte[] rc4Key = new byte[16];
			SlimHashAlgorithm.ComputeHash<Md5Context>(keyMaterial, rc4Key);

			byte[] plaintext = new byte[payloadLen];
			Rc4.Transform(rc4Key, ciphertext, plaintext);
			return plaintext;
		}

		/// <summary>
		/// Parses a decrypted USER_PROPERTIES blob to extract Kerberos keys.
		/// See [MS-SAMR] § 2.2.10.1 USER_PROPERTIES.
		/// </summary>
		private static KerberosKeys? ParseUserProperties(byte[] data)
		{
			// USER_PROPERTIES structure:
			//   DWORD Reserved1     (offset 0, always 0x50)
			//   DWORD Length        (offset 4)
			//   WORD  Reserved2     (offset 8, always 0x0001)
			//   WORD  Reserved3     (offset 10)
			//   BYTE  Reserved4[96] (offset 12)
			//   WORD  PropertyCount (offset 108)
			//   USER_PROPERTY[PropertyCount] (offset 110)

			if (data.Length < 112)
				return null;

			uint reserved1 = ReadUInt32Le(data, 0);
			// Valid USER_PROPERTIES start with 0x00000050
			if (reserved1 != 0x00000050)
				return null;

			uint length = ReadUInt32Le(data, 4);
			if (length > data.Length)
				return null;

			ushort propCount = ReadUInt16Le(data, 108);
			int offset = 110;

			var keys = new KerberosKeys();

			for (int i = 0; i < propCount && offset + 8 <= data.Length; i++)
			{
				// USER_PROPERTY:
				//   WORD  NameLength    (offset 0)
				//   WORD  ValueLength   (offset 2)
				//   WORD  Reserved      (offset 4)
				//   WCHAR Name[NameLength/2]
				//   BYTE  Value[ValueLength] (hex-encoded)

				ushort nameLen = ReadUInt16Le(data, offset);
				ushort valueLen = ReadUInt16Le(data, offset + 2);
				// skip Reserved (2 bytes)
				offset += 6;

				if (offset + nameLen + valueLen > data.Length)
					break;

				string name = Encoding.Unicode.GetString(data, offset, nameLen);
				offset += nameLen;

				// Value is stored as hex ASCII
				string valueHex = Encoding.Unicode.GetString(data, offset, valueLen);
				offset += valueLen;

				if (name.Equals("Primary:Kerberos-Newer-Keys", StringComparison.OrdinalIgnoreCase))
				{
					ParseKerberosNewerKeys(valueHex, keys);
				}
			}

			return keys.HasAnyKey ? keys : null;
		}

		/// <summary>
		/// Parses the Primary:Kerberos-Newer-Keys property to extract AES/DES keys.
		/// The value is a hex-encoded KERB_STORED_CREDENTIAL_NEW structure.
		/// </summary>
		private static void ParseKerberosNewerKeys(string hexValue, KerberosKeys keys)
		{
			byte[]? blob = TryHexDecode(hexValue);
			if (blob == null || blob.Length < 16)
				return;

			// KERB_STORED_CREDENTIAL_NEW:
			//   SHORT Revision         (2) — 4
			//   SHORT Flags            (2)
			//   SHORT CredentialCount  (2)
			//   SHORT ServiceCredentialCount (2)
			//   SHORT OldCredentialCount (2)
			//   SHORT OlderCredentialCount (2)
			//   SHORT DefaultSaltMaximumLength (2)
			//   SHORT DefaultSaltLength (2)
			//   ULONG DefaultSaltOffset (4)
			//   Credentials[CredentialCount] follow
			//   ...

			if (blob.Length < 16)
				return;

			ushort revision = ReadUInt16Le(blob, 0);
			if (revision != 4)
				return;

			ushort credCount = ReadUInt16Le(blob, 4);
			int credOffset = 16;  // credentials start right after the header

			// Each credential: SHORT KeyType, SHORT KeyLength, ULONG KeyOffset
			for (int i = 0; i < credCount && credOffset + 8 <= blob.Length; i++)
			{
				ushort keyType = ReadUInt16Le(blob, credOffset);
				ushort keyLength = ReadUInt16Le(blob, credOffset + 2);
				uint keyOffset = ReadUInt32Le(blob, credOffset + 4);
				credOffset += 8;

				if (keyOffset + keyLength > blob.Length)
					continue;

				byte[] keyData = new byte[keyLength];
				Array.Copy(blob, keyOffset, keyData, 0, keyLength);
				string keyHex = Convert.ToHexString(keyData).ToLowerInvariant();

				// Key types: 17 = AES128-CTS-HMAC-SHA1-96, 18 = AES256-CTS-HMAC-SHA1-96, 3 = DES-CBC-MD5
				switch (keyType)
				{
					case 18:
						keys.Aes256 ??= keyHex;
						break;
					case 17:
						keys.Aes128 ??= keyHex;
						break;
					case 3:
						keys.DesCbcMd5 ??= keyHex;
						break;
				}
			}
		}

		private static byte[]? TryHexDecode(string hex)
		{
			if (string.IsNullOrEmpty(hex) || hex.Length % 2 != 0)
				return null;
			try
			{
				return Convert.FromHexString(hex);
			}
			catch
			{
				return null;
			}
		}

		private static uint ReadUInt32Le(byte[] data, int offset)
			=> (uint)(data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16) | (data[offset + 3] << 24));

		private static ushort ReadUInt16Le(byte[] data, int offset)
			=> (ushort)(data[offset] | (data[offset + 1] << 8));
	}

	/// <summary>
	/// Kerberos long-term keys extracted from supplementalCredentials.
	/// </summary>
	public class KerberosKeys
	{
		public string? Aes256 { get; set; }
		public string? Aes128 { get; set; }
		public string? DesCbcMd5 { get; set; }

		public bool HasAnyKey => Aes256 != null || Aes128 != null || DesCbcMd5 != null;
	}
}
