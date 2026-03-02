using ms_drsr;
using System;
using System.Collections.Generic;
using System.Text;
using Titanis.Winterop.Security;

namespace Titanis.Msrpc.Msdrsr
{
	/// <summary>
	/// Decodes raw <see cref="ATTRBLOCK"/> attribute data into named fields,
	/// resolving attribute type IDs via a <see cref="SCHEMA_PREFIX_TABLE"/>.
	/// </summary>
	public static class DrsAttributeDecoder
	{
		// -------------------------------------------------------------------------
		// Well-known attribute OID suffixes (last arc) for common attributes.
		// The full attrTyp = (prefixTableEntry.ndx << 16) | lastArc
		// where lastArc is the final OID component (< 0x8000) or encoded (>= 0x8000).
		// -------------------------------------------------------------------------

		// OID 1.2.840.113556.1.4.x  →  prefix "1.2.840.113556.1.4"
		// OID 2.5.4.x                →  prefix "2.5.4"

		private const uint OidSuffix_unicodePwd              = 90;   // 1.2.840.113556.1.4.90
		private const uint OidSuffix_dBCSPwd                 = 55;   // 1.2.840.113556.1.4.55
		private const uint OidSuffix_supplementalCredentials = 125;  // 1.2.840.113556.1.4.125
		private const uint OidSuffix_sAMAccountName          = 221;  // 1.2.840.113556.1.4.221
		private const uint OidSuffix_userPrincipalName       = 656;  // 1.2.840.113556.1.4.656
		private const uint OidSuffix_objectSid               = 45;   // 2.5.4.45

		// BER-encoded OID prefixes for the known OID families
		// 1.2.840.113556.1.4 → 2a 86 48 86 f7 14 01 04  (8 bytes)
		private static readonly byte[] PrefixOid_ms_1_2_840_113556_1_4 =
			new byte[] { 0x55, 0x04 };  // placeholder; real prefix derived below

		// Cached prefix indices per SCHEMA_PREFIX_TABLE pointer identity.
		// The cache is keyed by SCHEMA_PREFIX_TABLE reference and is reset per call.
		// We rebuild it fresh on each decode call since it's cheap.

		/// <summary>
		/// Decodes a block of replicated attributes into a structured <see cref="DecodedAttributes"/>.
		/// </summary>
		public static DecodedAttributes Decode(in ATTRBLOCK attrBlock, in SCHEMA_PREFIX_TABLE prefixTable)
		{
			// Build the OID lookup: attrTyp → well-known field name
			var attrMap = BuildAttrMap(in prefixTable);

			var result = new DecodedAttributes();

			if (attrBlock.pAttr?.value == null)
				return result;

			foreach (var attr in attrBlock.pAttr.value)
			{
				if (!attrMap.TryGetValue(attr.attrTyp, out var fieldName))
					continue;

				byte[]? firstVal = GetFirstValue(attr);
				if (firstVal == null)
					continue;

				switch (fieldName)
				{
					case "unicodePwd":
						result.EncryptedNtHash = firstVal;
						break;
					case "dBCSPwd":
						result.EncryptedLmHash = firstVal;
						break;
					case "supplementalCredentials":
						result.EncryptedSupplementalCredentials = firstVal;
						break;
					case "sAMAccountName":
						result.SamAccountName = Encoding.Unicode.GetString(firstVal);
						break;
					case "userPrincipalName":
						result.UserPrincipalName = Encoding.Unicode.GetString(firstVal);
						break;
					case "objectSid":
						result.ObjectSidBytes = firstVal;
						break;
				}
			}

			return result;
		}

		private static byte[]? GetFirstValue(in ATTR attr)
		{
			if (attr.AttrVal.valCount == 0)
				return null;
			var pAVal = attr.AttrVal.pAVal;
			if (pAVal?.value == null || pAVal.value.Length == 0)
				return null;

			var firstVal = pAVal.value[0];
			if (firstVal.pVal?.value == null)
				return null;

			// Trim to declared length
			var raw = firstVal.pVal.value;
			int len = (int)firstVal.valLen;
			if (len > raw.Length)
				len = raw.Length;

			byte[] trimmed = new byte[len];
			Array.Copy(raw, trimmed, len);
			return trimmed;
		}

		/// <summary>
		/// Builds a mapping from attrTyp (uint) → field name string,
		/// by iterating the schema prefix table and comparing BER-encoded OID prefixes.
		/// </summary>
		private static Dictionary<uint, string> BuildAttrMap(in SCHEMA_PREFIX_TABLE prefixTable)
		{
			var map = new Dictionary<uint, string>();

			if (prefixTable.pPrefixEntry?.value == null)
				return map;

			foreach (var entry in prefixTable.pPrefixEntry.value)
			{
				byte[]? prefixBytes = entry.prefix.elements?.value;
				if (prefixBytes == null)
					continue;

				uint ndx = entry.ndx;

				// Try to match each known attribute to this prefix entry
				TryAddAttr(map, ndx, prefixBytes, OidSuffix_unicodePwd,              "1.2.840.113556.1.4", "unicodePwd");
				TryAddAttr(map, ndx, prefixBytes, OidSuffix_dBCSPwd,                 "1.2.840.113556.1.4", "dBCSPwd");
				TryAddAttr(map, ndx, prefixBytes, OidSuffix_supplementalCredentials,  "1.2.840.113556.1.4", "supplementalCredentials");
				TryAddAttr(map, ndx, prefixBytes, OidSuffix_sAMAccountName,          "1.2.840.113556.1.4", "sAMAccountName");
				TryAddAttr(map, ndx, prefixBytes, OidSuffix_userPrincipalName,       "1.2.840.113556.1.4", "userPrincipalName");
				TryAddAttr(map, ndx, prefixBytes, OidSuffix_objectSid,               "2.5.4",               "objectSid");
			}

			return map;
		}

		private static void TryAddAttr(
			Dictionary<uint, string> map,
			uint ndx,
			byte[] prefixBytes,
			uint oidSuffix,
			string oidFamily,
			string fieldName)
		{
			byte[] expectedPrefix = GetBerEncodedOidPrefix(oidFamily);
			if (!PrefixMatches(prefixBytes, expectedPrefix))
				return;

			// attrTyp = (ndx << 16) | last_arc
			// For last arc < 128 it is a single byte; encode it
			uint attrTyp;
			if (oidSuffix < 0x80)
			{
				attrTyp = (ndx << 16) | oidSuffix;
			}
			else if (oidSuffix < 0x4000)
			{
				// 2-byte BER encoding: high bit set in first byte
				uint berHigh = 0x80 | (oidSuffix >> 7);
				uint berLow = oidSuffix & 0x7F;
				attrTyp = (ndx << 16) | (berHigh << 8) | berLow;
			}
			else
			{
				// 3-byte for completeness (shouldn't be needed for our attributes)
				return;
			}

			map.TryAdd(attrTyp, fieldName);
		}

		private static bool PrefixMatches(byte[] actual, byte[] expected)
		{
			if (actual.Length != expected.Length)
				return false;
			for (int i = 0; i < actual.Length; i++)
			{
				if (actual[i] != expected[i])
					return false;
			}
			return true;
		}

		// Cache of known BER-encoded OID prefixes
		private static readonly Dictionary<string, byte[]> _berPrefixCache = new();

		private static byte[] GetBerEncodedOidPrefix(string oidDotNotation)
		{
			if (_berPrefixCache.TryGetValue(oidDotNotation, out byte[]? cached))
				return cached;

			byte[] encoded = EncodeBerOidPrefix(oidDotNotation);
			_berPrefixCache[oidDotNotation] = encoded;
			return encoded;
		}

		/// <summary>
		/// Encodes a dotted OID string into the BER prefix format used by the MS-DRSR
		/// prefix table (all arcs encoded, last arc NOT included since it's in attrTyp).
		/// </summary>
		private static byte[] EncodeBerOidPrefix(string oidDotNotation)
		{
			string[] parts = oidDotNotation.Split('.');
			if (parts.Length < 2)
				return Array.Empty<byte>();

			var bytes = new List<byte>();

			// First two arcs: (arc0 * 40 + arc1) as a single BER integer
			uint first = uint.Parse(parts[0]) * 40 + uint.Parse(parts[1]);
			EncodeBerArc(bytes, first);

			// Remaining arcs
			for (int i = 2; i < parts.Length; i++)
			{
				EncodeBerArc(bytes, uint.Parse(parts[i]));
			}

			return bytes.ToArray();
		}

		private static void EncodeBerArc(List<byte> bytes, uint value)
		{
			if (value < 0x80)
			{
				bytes.Add((byte)value);
			}
			else if (value < 0x4000)
			{
				bytes.Add((byte)(0x80 | (value >> 7)));
				bytes.Add((byte)(value & 0x7F));
			}
			else if (value < 0x200000)
			{
				bytes.Add((byte)(0x80 | (value >> 14)));
				bytes.Add((byte)(0x80 | ((value >> 7) & 0x7F)));
				bytes.Add((byte)(value & 0x7F));
			}
			else
			{
				bytes.Add((byte)(0x80 | (value >> 21)));
				bytes.Add((byte)(0x80 | ((value >> 14) & 0x7F)));
				bytes.Add((byte)(0x80 | ((value >> 7) & 0x7F)));
				bytes.Add((byte)(value & 0x7F));
			}
		}
	}

	/// <summary>
	/// Decoded attribute data for a single replicated object.
	/// </summary>
	public class DecodedAttributes
	{
		public string? SamAccountName { get; set; }
		public string? UserPrincipalName { get; set; }
		public byte[]? ObjectSidBytes { get; set; }
		public byte[]? EncryptedNtHash { get; set; }
		public byte[]? EncryptedLmHash { get; set; }
		public byte[]? EncryptedSupplementalCredentials { get; set; }

		public string? GetSidString()
		{
			if (ObjectSidBytes == null || ObjectSidBytes.Length < 8)
				return null;
			try
			{
				return new SecurityIdentifier(ObjectSidBytes.AsSpan()).ToString();
			}
			catch
			{
				return null;
			}
		}
	}
}
