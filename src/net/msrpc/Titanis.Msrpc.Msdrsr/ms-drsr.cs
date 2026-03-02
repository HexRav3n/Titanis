#pragma warning disable

// [MS-DRSR]: Directory Replication Service (DRS) Remote Protocol
// Interface UUID: e3514235-4b06-11d1-ab04-00c04fc2dcd2, version 4.0

namespace ms_drsr {
    using System;
    using System.Threading.Tasks;
    using Titanis;
    using Titanis.DceRpc;

    // -------------------------------------------------------------------------
    // Enumerations
    // -------------------------------------------------------------------------

    [System.CodeDom.Compiler.GeneratedCodeAttribute("Animus IDL Compiler", "0.9.4")]
    [Flags]
    public enum DRS_OPTIONS : uint {
        DRS_ASYNC_OP                = 0x00000001,
        DRS_GETCHG_CHECK            = 0x00000002,
        DRS_UPDATE_NOTIFICATION     = 0x00000004,
        DRS_ADD_REF                 = 0x00000008,
        DRS_SYNC_ALL                = 0x00000010,
        DRS_DEL_REF                 = 0x00000020,
        DRS_WRIT_REP                = 0x00000040,
        DRS_INIT_SYNC               = 0x00000080,
        DRS_PER_SYNC                = 0x00000100,
        DRS_MAIL_REP                = 0x00000200,
        DRS_ASYNC_REP               = 0x00000400,
        DRS_IGNORE_ERROR            = 0x00000800,
        DRS_TWOWAY_SYNC             = 0x00001000,
        DRS_CRITICAL_ONLY           = 0x00002000,
        DRS_GET_ANC                 = 0x00004000,
        DRS_GET_NC_SIZE             = 0x00008000,
        DRS_LOCAL_ONLY              = 0x00010000,
        DRS_NONGC_RO_REP            = 0x00040000,
        DRS_SYNC_BYNAME             = 0x00080000,
        DRS_REF_OK                  = 0x00100000,
        DRS_FULL_SYNC_NOW           = 0x00200000,
        DRS_NO_SOURCE               = 0x00400000,
        DRS_FULL_SYNC_IN_PROGRESS   = 0x00800000,
        DRS_FULL_SYNC_PACKET        = 0x01000000,
        DRS_SYNC_REQUEUE            = 0x02000000,
        DRS_SYNC_URGENT             = 0x04000000,
        DRS_REF_GCSPN               = 0x00000100,
        DRS_NO_DISCARD              = 0x08000000,
        DRS_NEVER_NOTIFY            = 0x10000000,
        DRS_PREEMPTED               = 0x20000000,
        DRS_SYNC_FORCED             = 0x40000000,
        DRS_DISABLE_AUTO_SYNC       = 0x80000000,
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("Animus IDL Compiler", "0.9.4")]
    [Flags]
    public enum DRS_EXTENSIONS_IN_FLAGS : uint {
        DRS_EXT_BASE                            = 0x00000001,
        DRS_EXT_ASYNCREPL                       = 0x00000002,
        DRS_EXT_REMOVEAPI                       = 0x00000004,
        DRS_EXT_MOVEREQ_V2                      = 0x00000008,
        DRS_EXT_GETCHG_DEFLATE                  = 0x00000010,
        DRS_EXT_DCINFO_V1                       = 0x00000020,
        DRS_EXT_RESTORE_USN_OPTIMIZATION        = 0x00000040,
        DRS_EXT_ADDENTRY                        = 0x00000080,
        DRS_EXT_KCC_EXECUTE                     = 0x00000100,
        DRS_EXT_ADDENTRY_V2                     = 0x00000200,
        DRS_EXT_LINKED_VALUE_REPLICATION        = 0x00000400,
        DRS_EXT_DCINFO_V2                       = 0x00000800,
        DRS_EXT_INSTANCE_TYPE_NOT_REQ_ON_MOD    = 0x00001000,
        DRS_EXT_CRYPTO_BIND                     = 0x00002000,
        DRS_EXT_GET_REPL_INFO                   = 0x00004000,
        DRS_EXT_STRONG_ENCRYPTION               = 0x00008000,
        DRS_EXT_DCINFO_VFFFFFFFF                = 0x00010000,
        DRS_EXT_TRANSITIVE_MEMBERSHIP           = 0x00020000,
        DRS_EXT_ADD_SID_HISTORY                 = 0x00040000,
        DRS_EXT_POST_BETA3                      = 0x00080000,
        DRS_EXT_GETCHGREPLY_V5                  = 0x00100000,
        DRS_EXT_GETMEMBERSHIPS2                 = 0x00200000,
        DRS_EXT_GETCHGREPLY_V6                  = 0x00400000,
        DRS_EXT_NONDOMAIN_NCS                   = 0x00800000,
        DRS_EXT_GETCHGREQ_V8                    = 0x01000000,
        DRS_EXT_GETCHGREPLY_V7                  = 0x02000000,
        DRS_EXT_VERIFY_OBJECT                   = 0x04000000,
        DRS_EXT_XP_DECODE                       = 0x08000000,
        DRS_EXT_NO_LARGE_OBJECTS                = 0x10000000,
        DRS_EXT_GET_MEMBERSHIPS2                = 0x20000000,
        DRS_EXT_GETCHGREQ_V10                   = 0x40000000,
        DRS_EXT_RESERVED_FOR_WIN2K_OR_DOTNET    = 0x80000000,
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("Animus IDL Compiler", "0.9.4")]
    public enum DS_NAME_FLAGS : uint {
        DS_NAME_NO_FLAGS            = 0x0,
        DS_NAME_FLAG_SYNTACTICAL_ONLY = 0x1,
        DS_NAME_FLAG_EVAL_AT_DC     = 0x2,
        DS_NAME_FLAG_GCVERIFY       = 0x4,
        DS_NAME_FLAG_TRUST_REFERRAL = 0x8,
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("Animus IDL Compiler", "0.9.4")]
    public enum DS_NAME_FORMAT : uint {
        DS_UNKNOWN_NAME             = 0,
        DS_FQDN_1779_NAME           = 1,
        DS_NT4_ACCOUNT_NAME         = 2,
        DS_DISPLAY_NAME             = 3,
        DS_UNIQUE_ID_NAME           = 6,
        DS_CANONICAL_NAME           = 7,
        DS_USER_PRINCIPAL_NAME      = 8,
        DS_CANONICAL_NAME_EX        = 9,
        DS_SERVICE_PRINCIPAL_NAME   = 10,
        DS_SID_OR_SID_HISTORY_NAME  = 11,
        DS_DNS_DOMAIN_NAME          = 12,
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("Animus IDL Compiler", "0.9.4")]
    public enum DS_NAME_ERROR : uint {
        DS_NAME_NO_ERROR                = 0,
        DS_NAME_ERROR_RESOLVING         = 1,
        DS_NAME_ERROR_NOT_FOUND         = 2,
        DS_NAME_ERROR_NOT_UNIQUE        = 3,
        DS_NAME_ERROR_NO_MAPPING        = 4,
        DS_NAME_ERROR_DOMAIN_ONLY       = 5,
        DS_NAME_ERROR_NO_SYNTACTICAL_MAPPING = 6,
        DS_NAME_ERROR_TRUST_REFERRAL    = 7,
    }

    // -------------------------------------------------------------------------
    // Core structures
    // -------------------------------------------------------------------------

    /// <summary>
    /// DRS extension capabilities blob. [MS-DRSR] § 5.38
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Animus IDL Compiler", "0.9.4")]
    public struct DRS_EXTENSIONS : Titanis.DceRpc.IRpcConformantStruct {
        public void Encode(Titanis.DceRpc.IRpcEncoder encoder) {
            // no fixed fields besides the conformant array
        }
        public void Decode(Titanis.DceRpc.IRpcDecoder decoder) {
        }
        public void EncodeHeader(Titanis.DceRpc.IRpcEncoder encoder) {
            encoder.WriteArrayHeader(this.rgb);
        }
        public void DecodeHeader(Titanis.DceRpc.IRpcDecoder decoder) {
            this.rgb = decoder.ReadArrayHeader<byte>();
        }
        public void EncodeConformantArrayField(Titanis.DceRpc.IRpcEncoder encoder) {
            for (int i = 0; (i < this.rgb.Length); i++) {
                encoder.WriteValue(this.rgb[i]);
            }
        }
        public void DecodeConformantArrayField(Titanis.DceRpc.IRpcDecoder decoder) {
            for (int i = 0; (i < this.rgb.Length); i++) {
                this.rgb[i] = decoder.ReadUnsignedChar();
            }
        }
        public byte[] rgb;
        public void EncodeDeferrals(Titanis.DceRpc.IRpcEncoder encoder) {
        }
        public void DecodeDeferrals(Titanis.DceRpc.IRpcDecoder decoder) {
        }
    }

    /// <summary>
    /// USN vector for replication state tracking. [MS-DRSR] § 5.206
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Animus IDL Compiler", "0.9.4")]
    public struct USN_VECTOR : Titanis.DceRpc.IRpcFixedStruct {
        public void Encode(Titanis.DceRpc.IRpcEncoder encoder) {
            encoder.WriteValue(this.usnHighObjUpdate);
            encoder.WriteValue(this.usnReserved);
            encoder.WriteValue(this.usnHighPropUpdate);
        }
        public void Decode(Titanis.DceRpc.IRpcDecoder decoder) {
            this.usnHighObjUpdate = decoder.ReadInt64();
            this.usnReserved = decoder.ReadInt64();
            this.usnHighPropUpdate = decoder.ReadInt64();
        }
        public long usnHighObjUpdate;
        public long usnReserved;
        public long usnHighPropUpdate;
        public void EncodeDeferrals(Titanis.DceRpc.IRpcEncoder encoder) {
        }
        public void DecodeDeferrals(Titanis.DceRpc.IRpcDecoder decoder) {
        }
    }

    /// <summary>
    /// Up-to-date vector cursor entry. [MS-DRSR] § 5.208
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Animus IDL Compiler", "0.9.4")]
    public struct UPTODATE_CURSOR_V1 : Titanis.DceRpc.IRpcFixedStruct {
        public void Encode(Titanis.DceRpc.IRpcEncoder encoder) {
            encoder.WriteFixedStruct(this.uuidDsa, Titanis.DceRpc.NdrAlignment._4Byte);
            encoder.WriteValue(this.usnHighPropUpdate);
        }
        public void Decode(Titanis.DceRpc.IRpcDecoder decoder) {
            this.uuidDsa = decoder.ReadFixedStruct<ms_dtyp.GUID>(Titanis.DceRpc.NdrAlignment._4Byte);
            this.usnHighPropUpdate = decoder.ReadInt64();
        }
        public ms_dtyp.GUID uuidDsa;
        public long usnHighPropUpdate;
        public void EncodeDeferrals(Titanis.DceRpc.IRpcEncoder encoder) {
        }
        public void DecodeDeferrals(Titanis.DceRpc.IRpcDecoder decoder) {
        }
    }

    /// <summary>
    /// Up-to-date vector (version 1). [MS-DRSR] § 5.211
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Animus IDL Compiler", "0.9.4")]
    public struct UPTODATE_VECTOR_V1_EXT : Titanis.DceRpc.IRpcFixedStruct {
        public void Encode(Titanis.DceRpc.IRpcEncoder encoder) {
            encoder.WriteValue(this.dwVersion);
            encoder.WriteValue(this.dwReserved1);
            encoder.WriteValue(this.cNumCursors);
            if ((this.rgCursors == null)) {
                this.rgCursors = new UPTODATE_CURSOR_V1[this.cNumCursors];
            }
            for (int i = 0; (i < this.cNumCursors); i++) {
                encoder.WriteFixedStruct(this.rgCursors[i], Titanis.DceRpc.NdrAlignment._4Byte);
            }
        }
        public void Decode(Titanis.DceRpc.IRpcDecoder decoder) {
            this.dwVersion = decoder.ReadUInt32();
            this.dwReserved1 = decoder.ReadUInt32();
            this.cNumCursors = decoder.ReadUInt32();
            this.rgCursors = new UPTODATE_CURSOR_V1[this.cNumCursors];
            for (int i = 0; (i < this.cNumCursors); i++) {
                this.rgCursors[i] = decoder.ReadFixedStruct<UPTODATE_CURSOR_V1>(Titanis.DceRpc.NdrAlignment._4Byte);
            }
        }
        public uint dwVersion;
        public uint dwReserved1;
        public uint cNumCursors;
        public UPTODATE_CURSOR_V1[] rgCursors;
        public void EncodeDeferrals(Titanis.DceRpc.IRpcEncoder encoder) {
        }
        public void DecodeDeferrals(Titanis.DceRpc.IRpcDecoder decoder) {
        }
    }

    /// <summary>
    /// OID binary encoding. [MS-DRSR] § 5.140
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Animus IDL Compiler", "0.9.4")]
    public struct OID_t : Titanis.DceRpc.IRpcFixedStruct {
        public void Encode(Titanis.DceRpc.IRpcEncoder encoder) {
            encoder.WriteValue(this.length);
            encoder.WritePointer(this.elements);
        }
        public void Decode(Titanis.DceRpc.IRpcDecoder decoder) {
            this.length = decoder.ReadUInt32();
            this.elements = decoder.ReadPointer<byte[]>();
        }
        public uint length;
        public RpcPointer<byte[]> elements;
        public void EncodeDeferrals(Titanis.DceRpc.IRpcEncoder encoder) {
            if ((null != this.elements)) {
                encoder.WriteArrayHeader<byte>(this.elements.value);
                for (int i = 0; (i < this.elements.value.Length); i++) {
                    encoder.WriteValue(this.elements.value[i]);
                }
            }
        }
        public void DecodeDeferrals(Titanis.DceRpc.IRpcDecoder decoder) {
            if ((null != this.elements)) {
                this.elements.value = decoder.ReadArrayHeader<byte>();
                for (int i = 0; (i < this.elements.value.Length); i++) {
                    this.elements.value[i] = decoder.ReadUnsignedChar();
                }
            }
        }
    }

    /// <summary>
    /// Prefix table entry mapping compressed OID prefix to index. [MS-DRSR] § 5.145
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Animus IDL Compiler", "0.9.4")]
    public struct PrefixTableEntry : Titanis.DceRpc.IRpcFixedStruct {
        public void Encode(Titanis.DceRpc.IRpcEncoder encoder) {
            encoder.WriteValue(this.ndx);
            encoder.WriteFixedStruct(this.prefix, Titanis.DceRpc.NdrAlignment._4Byte);
            encoder.WriteStructDeferral(this.prefix);
        }
        public void Decode(Titanis.DceRpc.IRpcDecoder decoder) {
            this.ndx = decoder.ReadUInt32();
            this.prefix = decoder.ReadFixedStruct<OID_t>(Titanis.DceRpc.NdrAlignment._4Byte);
        }
        public uint ndx;
        public OID_t prefix;
        public void EncodeDeferrals(Titanis.DceRpc.IRpcEncoder encoder) {
            encoder.WriteStructDeferral(this.prefix);
        }
        public void DecodeDeferrals(Titanis.DceRpc.IRpcDecoder decoder) {
            decoder.ReadStructDeferral<OID_t>(ref this.prefix);
        }
    }

    /// <summary>
    /// Schema prefix table. [MS-DRSR] § 5.145
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Animus IDL Compiler", "0.9.4")]
    public struct SCHEMA_PREFIX_TABLE : Titanis.DceRpc.IRpcFixedStruct {
        public void Encode(Titanis.DceRpc.IRpcEncoder encoder) {
            encoder.WriteValue(this.PrefixCount);
            encoder.WritePointer(this.pPrefixEntry);
        }
        public void Decode(Titanis.DceRpc.IRpcDecoder decoder) {
            this.PrefixCount = decoder.ReadUInt32();
            this.pPrefixEntry = decoder.ReadPointer<PrefixTableEntry[]>();
        }
        public uint PrefixCount;
        public RpcPointer<PrefixTableEntry[]> pPrefixEntry;
        public void EncodeDeferrals(Titanis.DceRpc.IRpcEncoder encoder) {
            if ((null != this.pPrefixEntry)) {
                encoder.WriteArrayHeader<PrefixTableEntry>(this.pPrefixEntry.value);
                for (int i = 0; (i < this.pPrefixEntry.value.Length); i++) {
                    encoder.WriteFixedStruct(this.pPrefixEntry.value[i], Titanis.DceRpc.NdrAlignment._4Byte);
                    encoder.WriteStructDeferral(this.pPrefixEntry.value[i]);
                }
                for (int i = 0; (i < this.pPrefixEntry.value.Length); i++) {
                    encoder.WriteStructDeferral(this.pPrefixEntry.value[i]);
                }
            }
        }
        public void DecodeDeferrals(Titanis.DceRpc.IRpcDecoder decoder) {
            if ((null != this.pPrefixEntry)) {
                this.pPrefixEntry.value = decoder.ReadArrayHeader<PrefixTableEntry>();
                for (int i = 0; (i < this.pPrefixEntry.value.Length); i++) {
                    this.pPrefixEntry.value[i] = decoder.ReadFixedStruct<PrefixTableEntry>(Titanis.DceRpc.NdrAlignment._4Byte);
                }
                for (int i = 0; (i < this.pPrefixEntry.value.Length); i++) {
                    decoder.ReadStructDeferral<PrefixTableEntry>(ref this.pPrefixEntry.value[i]);
                }
            }
        }
    }

    /// <summary>
    /// Partial attribute vector (version 1 extended). [MS-DRSR] § 5.143
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Animus IDL Compiler", "0.9.4")]
    public struct PARTIAL_ATTR_VECTOR_V1_EXT : Titanis.DceRpc.IRpcFixedStruct {
        public void Encode(Titanis.DceRpc.IRpcEncoder encoder) {
            encoder.WriteValue(this.dwVersion);
            encoder.WriteValue(this.dwReserved1);
            encoder.WriteValue(this.cAttrs);
            if ((this.rgPartialAttr == null)) {
                this.rgPartialAttr = new uint[this.cAttrs];
            }
            for (int i = 0; (i < this.cAttrs); i++) {
                encoder.WriteValue(this.rgPartialAttr[i]);
            }
        }
        public void Decode(Titanis.DceRpc.IRpcDecoder decoder) {
            this.dwVersion = decoder.ReadUInt32();
            this.dwReserved1 = decoder.ReadUInt32();
            this.cAttrs = decoder.ReadUInt32();
            this.rgPartialAttr = new uint[this.cAttrs];
            for (int i = 0; (i < this.cAttrs); i++) {
                this.rgPartialAttr[i] = decoder.ReadUInt32();
            }
        }
        public uint dwVersion;
        public uint dwReserved1;
        public uint cAttrs;
        public uint[] rgPartialAttr;
        public void EncodeDeferrals(Titanis.DceRpc.IRpcEncoder encoder) {
        }
        public void DecodeDeferrals(Titanis.DceRpc.IRpcDecoder decoder) {
        }
    }

    /// <summary>
    /// Directory Service object name. [MS-DRSR] § 5.51
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Animus IDL Compiler", "0.9.4")]
    public struct DSNAME : Titanis.DceRpc.IRpcConformantStruct {
        public void Encode(Titanis.DceRpc.IRpcEncoder encoder) {
            encoder.WriteValue(this.structLen);
            encoder.WriteValue(this.SidLen);
            encoder.WriteFixedStruct(this.Guid, Titanis.DceRpc.NdrAlignment._4Byte);
            // SID is a fixed 28-byte array
            if ((this.Sid == null)) {
                this.Sid = new byte[28];
            }
            for (int i = 0; (i < 28); i++) {
                encoder.WriteValue(this.Sid[i]);
            }
            encoder.WriteValue(this.NameLen);
        }
        public void Decode(Titanis.DceRpc.IRpcDecoder decoder) {
            this.structLen = decoder.ReadUInt32();
            this.SidLen = decoder.ReadUInt32();
            this.Guid = decoder.ReadFixedStruct<ms_dtyp.GUID>(Titanis.DceRpc.NdrAlignment._4Byte);
            this.Sid = new byte[28];
            for (int i = 0; (i < 28); i++) {
                this.Sid[i] = decoder.ReadUnsignedChar();
            }
            this.NameLen = decoder.ReadUInt32();
        }
        public void EncodeHeader(Titanis.DceRpc.IRpcEncoder encoder) {
            encoder.WriteArrayHeader(this.StringName);
        }
        public void DecodeHeader(Titanis.DceRpc.IRpcDecoder decoder) {
            this.StringName = decoder.ReadArrayHeader<char>();
        }
        public void EncodeConformantArrayField(Titanis.DceRpc.IRpcEncoder encoder) {
            for (int i = 0; (i < this.StringName.Length); i++) {
                encoder.WriteValue(this.StringName[i]);
            }
        }
        public void DecodeConformantArrayField(Titanis.DceRpc.IRpcDecoder decoder) {
            for (int i = 0; (i < this.StringName.Length); i++) {
                this.StringName[i] = (char)decoder.ReadChar();
            }
        }
        public uint structLen;
        public uint SidLen;
        public ms_dtyp.GUID Guid;
        public byte[] Sid;
        public uint NameLen;
        public char[] StringName;
        public void EncodeDeferrals(Titanis.DceRpc.IRpcEncoder encoder) {
        }
        public void DecodeDeferrals(Titanis.DceRpc.IRpcDecoder decoder) {
        }
    }

    /// <summary>
    /// Attribute value. [MS-DRSR] § 5.9
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Animus IDL Compiler", "0.9.4")]
    public struct ATTRVAL : Titanis.DceRpc.IRpcFixedStruct {
        public void Encode(Titanis.DceRpc.IRpcEncoder encoder) {
            encoder.WriteValue(this.valLen);
            encoder.WritePointer(this.pVal);
        }
        public void Decode(Titanis.DceRpc.IRpcDecoder decoder) {
            this.valLen = decoder.ReadUInt32();
            this.pVal = decoder.ReadPointer<byte[]>();
        }
        public uint valLen;
        public RpcPointer<byte[]> pVal;
        public void EncodeDeferrals(Titanis.DceRpc.IRpcEncoder encoder) {
            if ((null != this.pVal)) {
                encoder.WriteArrayHeader<byte>(this.pVal.value);
                for (int i = 0; (i < this.pVal.value.Length); i++) {
                    encoder.WriteValue(this.pVal.value[i]);
                }
            }
        }
        public void DecodeDeferrals(Titanis.DceRpc.IRpcDecoder decoder) {
            if ((null != this.pVal)) {
                this.pVal.value = decoder.ReadArrayHeader<byte>();
                for (int i = 0; (i < this.pVal.value.Length); i++) {
                    this.pVal.value[i] = decoder.ReadUnsignedChar();
                }
            }
        }
    }

    /// <summary>
    /// Block of attribute values. [MS-DRSR] § 5.8
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Animus IDL Compiler", "0.9.4")]
    public struct ATTRVALBLOCK : Titanis.DceRpc.IRpcFixedStruct {
        public void Encode(Titanis.DceRpc.IRpcEncoder encoder) {
            encoder.WriteValue(this.valCount);
            encoder.WritePointer(this.pAVal);
        }
        public void Decode(Titanis.DceRpc.IRpcDecoder decoder) {
            this.valCount = decoder.ReadUInt32();
            this.pAVal = decoder.ReadPointer<ATTRVAL[]>();
        }
        public uint valCount;
        public RpcPointer<ATTRVAL[]> pAVal;
        public void EncodeDeferrals(Titanis.DceRpc.IRpcEncoder encoder) {
            if ((null != this.pAVal)) {
                encoder.WriteArrayHeader<ATTRVAL>(this.pAVal.value);
                for (int i = 0; (i < this.pAVal.value.Length); i++) {
                    encoder.WriteFixedStruct(this.pAVal.value[i], Titanis.DceRpc.NdrAlignment._4Byte);
                }
                for (int i = 0; (i < this.pAVal.value.Length); i++) {
                    encoder.WriteStructDeferral(this.pAVal.value[i]);
                }
            }
        }
        public void DecodeDeferrals(Titanis.DceRpc.IRpcDecoder decoder) {
            if ((null != this.pAVal)) {
                this.pAVal.value = decoder.ReadArrayHeader<ATTRVAL>();
                for (int i = 0; (i < this.pAVal.value.Length); i++) {
                    this.pAVal.value[i] = decoder.ReadFixedStruct<ATTRVAL>(Titanis.DceRpc.NdrAlignment._4Byte);
                }
                for (int i = 0; (i < this.pAVal.value.Length); i++) {
                    decoder.ReadStructDeferral<ATTRVAL>(ref this.pAVal.value[i]);
                }
            }
        }
    }

    /// <summary>
    /// Single attribute with type and values. [MS-DRSR] § 5.7
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Animus IDL Compiler", "0.9.4")]
    public struct ATTR : Titanis.DceRpc.IRpcFixedStruct {
        public void Encode(Titanis.DceRpc.IRpcEncoder encoder) {
            encoder.WriteValue(this.attrTyp);
            encoder.WriteFixedStruct(this.AttrVal, Titanis.DceRpc.NdrAlignment._4Byte);
            encoder.WriteStructDeferral(this.AttrVal);
        }
        public void Decode(Titanis.DceRpc.IRpcDecoder decoder) {
            this.attrTyp = decoder.ReadUInt32();
            this.AttrVal = decoder.ReadFixedStruct<ATTRVALBLOCK>(Titanis.DceRpc.NdrAlignment._4Byte);
        }
        public uint attrTyp;
        public ATTRVALBLOCK AttrVal;
        public void EncodeDeferrals(Titanis.DceRpc.IRpcEncoder encoder) {
            encoder.WriteStructDeferral(this.AttrVal);
        }
        public void DecodeDeferrals(Titanis.DceRpc.IRpcDecoder decoder) {
            decoder.ReadStructDeferral<ATTRVALBLOCK>(ref this.AttrVal);
        }
    }

    /// <summary>
    /// Block of attributes. [MS-DRSR] § 5.6
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Animus IDL Compiler", "0.9.4")]
    public struct ATTRBLOCK : Titanis.DceRpc.IRpcFixedStruct {
        public void Encode(Titanis.DceRpc.IRpcEncoder encoder) {
            encoder.WriteValue(this.attrCount);
            encoder.WritePointer(this.pAttr);
        }
        public void Decode(Titanis.DceRpc.IRpcDecoder decoder) {
            this.attrCount = decoder.ReadUInt32();
            this.pAttr = decoder.ReadPointer<ATTR[]>();
        }
        public uint attrCount;
        public RpcPointer<ATTR[]> pAttr;
        public void EncodeDeferrals(Titanis.DceRpc.IRpcEncoder encoder) {
            if ((null != this.pAttr)) {
                encoder.WriteArrayHeader<ATTR>(this.pAttr.value);
                for (int i = 0; (i < this.pAttr.value.Length); i++) {
                    encoder.WriteFixedStruct(this.pAttr.value[i], Titanis.DceRpc.NdrAlignment._4Byte);
                    encoder.WriteStructDeferral(this.pAttr.value[i]);
                }
                for (int i = 0; (i < this.pAttr.value.Length); i++) {
                    encoder.WriteStructDeferral(this.pAttr.value[i]);
                }
            }
        }
        public void DecodeDeferrals(Titanis.DceRpc.IRpcDecoder decoder) {
            if ((null != this.pAttr)) {
                this.pAttr.value = decoder.ReadArrayHeader<ATTR>();
                for (int i = 0; (i < this.pAttr.value.Length); i++) {
                    this.pAttr.value[i] = decoder.ReadFixedStruct<ATTR>(Titanis.DceRpc.NdrAlignment._4Byte);
                }
                for (int i = 0; (i < this.pAttr.value.Length); i++) {
                    decoder.ReadStructDeferral<ATTR>(ref this.pAttr.value[i]);
                }
            }
        }
    }

    /// <summary>
    /// Entry info — object name + attributes. [MS-DRSR] § 5.55
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Animus IDL Compiler", "0.9.4")]
    public struct ENTINF : Titanis.DceRpc.IRpcFixedStruct {
        public void Encode(Titanis.DceRpc.IRpcEncoder encoder) {
            encoder.WritePointer(this.pName);
            encoder.WriteValue(this.ulFlags);
            encoder.WriteFixedStruct(this.AttrBlock, Titanis.DceRpc.NdrAlignment._4Byte);
            encoder.WriteStructDeferral(this.AttrBlock);
        }
        public void Decode(Titanis.DceRpc.IRpcDecoder decoder) {
            this.pName = decoder.ReadPointer<DSNAME>();
            this.ulFlags = decoder.ReadUInt32();
            this.AttrBlock = decoder.ReadFixedStruct<ATTRBLOCK>(Titanis.DceRpc.NdrAlignment._4Byte);
        }
        public RpcPointer<DSNAME> pName;
        public uint ulFlags;
        public ATTRBLOCK AttrBlock;
        public void EncodeDeferrals(Titanis.DceRpc.IRpcEncoder encoder) {
            if ((null != this.pName)) {
                encoder.WriteConformantStruct(this.pName.value, Titanis.DceRpc.NdrAlignment._4Byte);
                encoder.WriteStructDeferral(this.pName.value);
            }
            encoder.WriteStructDeferral(this.AttrBlock);
        }
        public void DecodeDeferrals(Titanis.DceRpc.IRpcDecoder decoder) {
            if ((null != this.pName)) {
                this.pName.value = decoder.ReadConformantStruct<DSNAME>(Titanis.DceRpc.NdrAlignment._4Byte);
                decoder.ReadStructDeferral<DSNAME>(ref this.pName.value);
            }
            decoder.ReadStructDeferral<ATTRBLOCK>(ref this.AttrBlock);
        }
    }

    /// <summary>
    /// Replicated entry info list node (linked list). [MS-DRSR] § 5.162
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Animus IDL Compiler", "0.9.4")]
    public struct REPLENTINFLIST : Titanis.DceRpc.IRpcFixedStruct {
        public void Encode(Titanis.DceRpc.IRpcEncoder encoder) {
            encoder.WritePointer(this.pNextEntInf);
            encoder.WriteFixedStruct(this.Entinf, Titanis.DceRpc.NdrAlignment._4Byte);
            encoder.WriteStructDeferral(this.Entinf);
            encoder.WriteValue(this.fIsNCPrefix);
            encoder.WritePointer(this.pParentGuid);
            encoder.WritePointer(this.pMetaDataExt);
        }
        public void Decode(Titanis.DceRpc.IRpcDecoder decoder) {
            this.pNextEntInf = decoder.ReadPointer<REPLENTINFLIST>();
            this.Entinf = decoder.ReadFixedStruct<ENTINF>(Titanis.DceRpc.NdrAlignment._4Byte);
            this.fIsNCPrefix = decoder.ReadBoolean();
            this.pParentGuid = decoder.ReadPointer<ms_dtyp.GUID>();
            this.pMetaDataExt = decoder.ReadPointer<byte[]>();
        }
        public RpcPointer<REPLENTINFLIST> pNextEntInf;
        public ENTINF Entinf;
        public bool fIsNCPrefix;
        public RpcPointer<ms_dtyp.GUID> pParentGuid;
        public RpcPointer<byte[]> pMetaDataExt;  // META_DATA_EXT opaque blob
        public void EncodeDeferrals(Titanis.DceRpc.IRpcEncoder encoder) {
            if ((null != this.pNextEntInf)) {
                encoder.WriteFixedStruct(this.pNextEntInf.value, Titanis.DceRpc.NdrAlignment._4Byte);
                encoder.WriteStructDeferral(this.pNextEntInf.value);
            }
            encoder.WriteStructDeferral(this.Entinf);
            if ((null != this.pParentGuid)) {
                encoder.WriteFixedStruct(this.pParentGuid.value, Titanis.DceRpc.NdrAlignment._4Byte);
            }
            if ((null != this.pMetaDataExt)) {
                encoder.WriteArrayHeader<byte>(this.pMetaDataExt.value);
                for (int i = 0; (i < this.pMetaDataExt.value.Length); i++) {
                    encoder.WriteValue(this.pMetaDataExt.value[i]);
                }
            }
        }
        public void DecodeDeferrals(Titanis.DceRpc.IRpcDecoder decoder) {
            if ((null != this.pNextEntInf)) {
                this.pNextEntInf.value = decoder.ReadFixedStruct<REPLENTINFLIST>(Titanis.DceRpc.NdrAlignment._4Byte);
                decoder.ReadStructDeferral<REPLENTINFLIST>(ref this.pNextEntInf.value);
            }
            decoder.ReadStructDeferral<ENTINF>(ref this.Entinf);
            if ((null != this.pParentGuid)) {
                this.pParentGuid.value = decoder.ReadFixedStruct<ms_dtyp.GUID>(Titanis.DceRpc.NdrAlignment._4Byte);
            }
            if ((null != this.pMetaDataExt)) {
                this.pMetaDataExt.value = decoder.ReadArrayHeader<byte>();
                for (int i = 0; (i < this.pMetaDataExt.value.Length); i++) {
                    this.pMetaDataExt.value[i] = decoder.ReadUnsignedChar();
                }
            }
        }
    }

    // -------------------------------------------------------------------------
    // GetNCChanges request/reply (V8 request, V6 reply)
    // -------------------------------------------------------------------------

    /// <summary>
    /// DRSGetNCChanges request version 8. [MS-DRSR] § 4.1.10.1.14
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Animus IDL Compiler", "0.9.4")]
    public struct DRS_MSG_GETCHGREQ_V8 : Titanis.DceRpc.IRpcFixedStruct {
        public void Encode(Titanis.DceRpc.IRpcEncoder encoder) {
            encoder.WriteFixedStruct(this.uuidDsaObjDest, Titanis.DceRpc.NdrAlignment._4Byte);
            encoder.WriteFixedStruct(this.uuidInvocIdSrc, Titanis.DceRpc.NdrAlignment._4Byte);
            encoder.WritePointer(this.pNC);
            encoder.WriteFixedStruct(this.usnvecFrom, Titanis.DceRpc.NdrAlignment._4Byte);
            encoder.WritePointer(this.pUpToDateVecDest);
            encoder.WriteValue(this.ulFlags);
            encoder.WriteValue(this.cMaxObjects);
            encoder.WriteValue(this.cMaxBytes);
            encoder.WriteValue(this.ulExtendedOp);
            encoder.WriteValue(this.liFsmoInfo);
            encoder.WritePointer(this.pPartialAttrSet);
            encoder.WritePointer(this.pPartialAttrSetEx);
            encoder.WriteFixedStruct(this.PrefixTableDest, Titanis.DceRpc.NdrAlignment._4Byte);
            encoder.WriteStructDeferral(this.PrefixTableDest);
        }
        public void Decode(Titanis.DceRpc.IRpcDecoder decoder) {
            this.uuidDsaObjDest = decoder.ReadFixedStruct<ms_dtyp.GUID>(Titanis.DceRpc.NdrAlignment._4Byte);
            this.uuidInvocIdSrc = decoder.ReadFixedStruct<ms_dtyp.GUID>(Titanis.DceRpc.NdrAlignment._4Byte);
            this.pNC = decoder.ReadPointer<DSNAME>();
            this.usnvecFrom = decoder.ReadFixedStruct<USN_VECTOR>(Titanis.DceRpc.NdrAlignment._4Byte);
            this.pUpToDateVecDest = decoder.ReadPointer<UPTODATE_VECTOR_V1_EXT>();
            this.ulFlags = decoder.ReadUInt32();
            this.cMaxObjects = decoder.ReadUInt32();
            this.cMaxBytes = decoder.ReadUInt32();
            this.ulExtendedOp = decoder.ReadUInt32();
            this.liFsmoInfo = decoder.ReadUInt64();
            this.pPartialAttrSet = decoder.ReadPointer<PARTIAL_ATTR_VECTOR_V1_EXT>();
            this.pPartialAttrSetEx = decoder.ReadPointer<PARTIAL_ATTR_VECTOR_V1_EXT>();
            this.PrefixTableDest = decoder.ReadFixedStruct<SCHEMA_PREFIX_TABLE>(Titanis.DceRpc.NdrAlignment._4Byte);
        }
        public ms_dtyp.GUID uuidDsaObjDest;
        public ms_dtyp.GUID uuidInvocIdSrc;
        public RpcPointer<DSNAME> pNC;
        public USN_VECTOR usnvecFrom;
        public RpcPointer<UPTODATE_VECTOR_V1_EXT> pUpToDateVecDest;
        public uint ulFlags;
        public uint cMaxObjects;
        public uint cMaxBytes;
        public uint ulExtendedOp;
        public ulong liFsmoInfo;
        public RpcPointer<PARTIAL_ATTR_VECTOR_V1_EXT> pPartialAttrSet;
        public RpcPointer<PARTIAL_ATTR_VECTOR_V1_EXT> pPartialAttrSetEx;
        public SCHEMA_PREFIX_TABLE PrefixTableDest;
        public void EncodeDeferrals(Titanis.DceRpc.IRpcEncoder encoder) {
            if ((null != this.pNC)) {
                encoder.WriteConformantStruct(this.pNC.value, Titanis.DceRpc.NdrAlignment._4Byte);
                encoder.WriteStructDeferral(this.pNC.value);
            }
            if ((null != this.pUpToDateVecDest)) {
                encoder.WriteFixedStruct(this.pUpToDateVecDest.value, Titanis.DceRpc.NdrAlignment._4Byte);
            }
            if ((null != this.pPartialAttrSet)) {
                encoder.WriteFixedStruct(this.pPartialAttrSet.value, Titanis.DceRpc.NdrAlignment._4Byte);
            }
            if ((null != this.pPartialAttrSetEx)) {
                encoder.WriteFixedStruct(this.pPartialAttrSetEx.value, Titanis.DceRpc.NdrAlignment._4Byte);
            }
            encoder.WriteStructDeferral(this.PrefixTableDest);
        }
        public void DecodeDeferrals(Titanis.DceRpc.IRpcDecoder decoder) {
            if ((null != this.pNC)) {
                this.pNC.value = decoder.ReadConformantStruct<DSNAME>(Titanis.DceRpc.NdrAlignment._4Byte);
                decoder.ReadStructDeferral<DSNAME>(ref this.pNC.value);
            }
            if ((null != this.pUpToDateVecDest)) {
                this.pUpToDateVecDest.value = decoder.ReadFixedStruct<UPTODATE_VECTOR_V1_EXT>(Titanis.DceRpc.NdrAlignment._4Byte);
            }
            if ((null != this.pPartialAttrSet)) {
                this.pPartialAttrSet.value = decoder.ReadFixedStruct<PARTIAL_ATTR_VECTOR_V1_EXT>(Titanis.DceRpc.NdrAlignment._4Byte);
            }
            if ((null != this.pPartialAttrSetEx)) {
                this.pPartialAttrSetEx.value = decoder.ReadFixedStruct<PARTIAL_ATTR_VECTOR_V1_EXT>(Titanis.DceRpc.NdrAlignment._4Byte);
            }
            decoder.ReadStructDeferral<SCHEMA_PREFIX_TABLE>(ref this.PrefixTableDest);
        }
    }

    /// <summary>
    /// DRSGetNCChanges reply version 6. [MS-DRSR] § 4.1.10.1.16
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Animus IDL Compiler", "0.9.4")]
    public struct DRS_MSG_GETCHGREPLY_V6 : Titanis.DceRpc.IRpcFixedStruct {
        public void Encode(Titanis.DceRpc.IRpcEncoder encoder) {
            encoder.WriteFixedStruct(this.uuidDsaObjSrc, Titanis.DceRpc.NdrAlignment._4Byte);
            encoder.WriteFixedStruct(this.uuidInvocIdSrc, Titanis.DceRpc.NdrAlignment._4Byte);
            encoder.WritePointer(this.pNC);
            encoder.WriteFixedStruct(this.usnvecFrom, Titanis.DceRpc.NdrAlignment._4Byte);
            encoder.WriteFixedStruct(this.usnvecTo, Titanis.DceRpc.NdrAlignment._4Byte);
            encoder.WritePointer(this.pUpToDateVecSrcV1);
            encoder.WriteFixedStruct(this.PrefixTableSrc, Titanis.DceRpc.NdrAlignment._4Byte);
            encoder.WriteStructDeferral(this.PrefixTableSrc);
            encoder.WriteValue(this.ulExtendedRet);
            encoder.WriteValue(this.cNumObjects);
            encoder.WriteValue(this.cNumBytes);
            encoder.WritePointer(this.pObjects);
            encoder.WriteValue(this.fMoreData);
            encoder.WriteValue(this.cNumNcSizeObjects);
            encoder.WriteValue(this.cNumNcSizeValues);
            encoder.WriteValue(this.cNumValues);
            encoder.WritePointer(this.rgValues);
            encoder.WriteValue(this.dwDRSError);
        }
        public void Decode(Titanis.DceRpc.IRpcDecoder decoder) {
            this.uuidDsaObjSrc = decoder.ReadFixedStruct<ms_dtyp.GUID>(Titanis.DceRpc.NdrAlignment._4Byte);
            this.uuidInvocIdSrc = decoder.ReadFixedStruct<ms_dtyp.GUID>(Titanis.DceRpc.NdrAlignment._4Byte);
            this.pNC = decoder.ReadPointer<DSNAME>();
            this.usnvecFrom = decoder.ReadFixedStruct<USN_VECTOR>(Titanis.DceRpc.NdrAlignment._4Byte);
            this.usnvecTo = decoder.ReadFixedStruct<USN_VECTOR>(Titanis.DceRpc.NdrAlignment._4Byte);
            this.pUpToDateVecSrcV1 = decoder.ReadPointer<UPTODATE_VECTOR_V1_EXT>();
            this.PrefixTableSrc = decoder.ReadFixedStruct<SCHEMA_PREFIX_TABLE>(Titanis.DceRpc.NdrAlignment._4Byte);
            this.ulExtendedRet = decoder.ReadUInt32();
            this.cNumObjects = decoder.ReadUInt32();
            this.cNumBytes = decoder.ReadUInt32();
            this.pObjects = decoder.ReadPointer<REPLENTINFLIST>();
            this.fMoreData = decoder.ReadBoolean();
            this.cNumNcSizeObjects = decoder.ReadUInt32();
            this.cNumNcSizeValues = decoder.ReadUInt32();
            this.cNumValues = decoder.ReadUInt32();
            this.rgValues = decoder.ReadPointer<byte[]>();
            this.dwDRSError = decoder.ReadUInt32();
        }
        public ms_dtyp.GUID uuidDsaObjSrc;
        public ms_dtyp.GUID uuidInvocIdSrc;
        public RpcPointer<DSNAME> pNC;
        public USN_VECTOR usnvecFrom;
        public USN_VECTOR usnvecTo;
        public RpcPointer<UPTODATE_VECTOR_V1_EXT> pUpToDateVecSrcV1;
        public SCHEMA_PREFIX_TABLE PrefixTableSrc;
        public uint ulExtendedRet;
        public uint cNumObjects;
        public uint cNumBytes;
        public RpcPointer<REPLENTINFLIST> pObjects;
        public bool fMoreData;
        public uint cNumNcSizeObjects;
        public uint cNumNcSizeValues;
        public uint cNumValues;
        public RpcPointer<byte[]> rgValues;  // VALUE_META_DATA_EXT_V1 array (opaque for our purposes)
        public uint dwDRSError;
        public void EncodeDeferrals(Titanis.DceRpc.IRpcEncoder encoder) {
            if ((null != this.pNC)) {
                encoder.WriteConformantStruct(this.pNC.value, Titanis.DceRpc.NdrAlignment._4Byte);
                encoder.WriteStructDeferral(this.pNC.value);
            }
            if ((null != this.pUpToDateVecSrcV1)) {
                encoder.WriteFixedStruct(this.pUpToDateVecSrcV1.value, Titanis.DceRpc.NdrAlignment._4Byte);
            }
            encoder.WriteStructDeferral(this.PrefixTableSrc);
            if ((null != this.pObjects)) {
                encoder.WriteFixedStruct(this.pObjects.value, Titanis.DceRpc.NdrAlignment._4Byte);
                encoder.WriteStructDeferral(this.pObjects.value);
            }
            if ((null != this.rgValues)) {
                encoder.WriteArrayHeader<byte>(this.rgValues.value);
                for (int i = 0; (i < this.rgValues.value.Length); i++) {
                    encoder.WriteValue(this.rgValues.value[i]);
                }
            }
        }
        public void DecodeDeferrals(Titanis.DceRpc.IRpcDecoder decoder) {
            if ((null != this.pNC)) {
                this.pNC.value = decoder.ReadConformantStruct<DSNAME>(Titanis.DceRpc.NdrAlignment._4Byte);
                decoder.ReadStructDeferral<DSNAME>(ref this.pNC.value);
            }
            if ((null != this.pUpToDateVecSrcV1)) {
                this.pUpToDateVecSrcV1.value = decoder.ReadFixedStruct<UPTODATE_VECTOR_V1_EXT>(Titanis.DceRpc.NdrAlignment._4Byte);
            }
            decoder.ReadStructDeferral<SCHEMA_PREFIX_TABLE>(ref this.PrefixTableSrc);
            if ((null != this.pObjects)) {
                this.pObjects.value = decoder.ReadFixedStruct<REPLENTINFLIST>(Titanis.DceRpc.NdrAlignment._4Byte);
                decoder.ReadStructDeferral<REPLENTINFLIST>(ref this.pObjects.value);
            }
            if ((null != this.rgValues)) {
                this.rgValues.value = decoder.ReadArrayHeader<byte>();
                for (int i = 0; (i < this.rgValues.value.Length); i++) {
                    this.rgValues.value[i] = decoder.ReadUnsignedChar();
                }
            }
        }
    }

    // -------------------------------------------------------------------------
    // CrackNames request/reply
    // -------------------------------------------------------------------------

    /// <summary>
    /// DRSCrackNames request version 1. [MS-DRSR] § 4.1.4.1.2
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Animus IDL Compiler", "0.9.4")]
    public struct DRS_MSG_CRACKREQ_V1 : Titanis.DceRpc.IRpcFixedStruct {
        public void Encode(Titanis.DceRpc.IRpcEncoder encoder) {
            encoder.WriteValue(this.CodePage);
            encoder.WriteValue(this.LocaleId);
            encoder.WriteValue(this.dwFlags);
            encoder.WriteValue(this.formatOffered);
            encoder.WriteValue(this.formatDesired);
            encoder.WriteValue(this.cNames);
            encoder.WritePointer(this.rpNames);
        }
        public void Decode(Titanis.DceRpc.IRpcDecoder decoder) {
            this.CodePage = decoder.ReadUInt32();
            this.LocaleId = decoder.ReadUInt32();
            this.dwFlags = decoder.ReadUInt32();
            this.formatOffered = decoder.ReadUInt32();
            this.formatDesired = decoder.ReadUInt32();
            this.cNames = decoder.ReadUInt32();
            this.rpNames = decoder.ReadPointer<string[]>();
        }
        public uint CodePage;
        public uint LocaleId;
        public uint dwFlags;
        public uint formatOffered;
        public uint formatDesired;
        public uint cNames;
        public RpcPointer<string[]> rpNames;
        public void EncodeDeferrals(Titanis.DceRpc.IRpcEncoder encoder) {
            if ((null != this.rpNames)) {
                encoder.WriteArrayHeader<string>(this.rpNames.value);
                for (int i = 0; (i < this.rpNames.value.Length); i++) {
                    encoder.WritePointer(new RpcPointer<string>(this.rpNames.value[i]));
                }
                for (int i = 0; (i < this.rpNames.value.Length); i++) {
                    if (this.rpNames.value[i] != null) {
                        encoder.WriteWideCharString(this.rpNames.value[i]);
                    }
                }
            }
        }
        public void DecodeDeferrals(Titanis.DceRpc.IRpcDecoder decoder) {
            if ((null != this.rpNames)) {
                this.rpNames.value = decoder.ReadArrayHeader<string>();
                var ptrs = new RpcPointer<string>[this.rpNames.value.Length];
                for (int i = 0; (i < this.rpNames.value.Length); i++) {
                    ptrs[i] = decoder.ReadPointer<string>();
                }
                for (int i = 0; (i < ptrs.Length); i++) {
                    if (ptrs[i] != null) {
                        this.rpNames.value[i] = decoder.ReadWideCharString();
                    }
                }
            }
        }
    }

    /// <summary>
    /// Single name translation result item. [MS-DRSR] § 4.1.4.1.7
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Animus IDL Compiler", "0.9.4")]
    public struct DS_NAME_RESULT_ITEMW : Titanis.DceRpc.IRpcFixedStruct {
        public void Encode(Titanis.DceRpc.IRpcEncoder encoder) {
            encoder.WriteValue(this.status);
            encoder.WritePointer(this.pDomain);
            encoder.WritePointer(this.pName);
        }
        public void Decode(Titanis.DceRpc.IRpcDecoder decoder) {
            this.status = decoder.ReadUInt32();
            this.pDomain = decoder.ReadPointer<string>();
            this.pName = decoder.ReadPointer<string>();
        }
        public uint status;
        public RpcPointer<string> pDomain;
        public RpcPointer<string> pName;
        public void EncodeDeferrals(Titanis.DceRpc.IRpcEncoder encoder) {
            if ((null != this.pDomain)) {
                encoder.WriteWideCharString(this.pDomain.value);
            }
            if ((null != this.pName)) {
                encoder.WriteWideCharString(this.pName.value);
            }
        }
        public void DecodeDeferrals(Titanis.DceRpc.IRpcDecoder decoder) {
            if ((null != this.pDomain)) {
                this.pDomain.value = decoder.ReadWideCharString();
            }
            if ((null != this.pName)) {
                this.pName.value = decoder.ReadWideCharString();
            }
        }
    }

    /// <summary>
    /// Name translation result. [MS-DRSR] § 4.1.4.1.6
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Animus IDL Compiler", "0.9.4")]
    public struct DS_NAME_RESULTW : Titanis.DceRpc.IRpcFixedStruct {
        public void Encode(Titanis.DceRpc.IRpcEncoder encoder) {
            encoder.WriteValue(this.cItems);
            encoder.WritePointer(this.rItems);
        }
        public void Decode(Titanis.DceRpc.IRpcDecoder decoder) {
            this.cItems = decoder.ReadUInt32();
            this.rItems = decoder.ReadPointer<DS_NAME_RESULT_ITEMW[]>();
        }
        public uint cItems;
        public RpcPointer<DS_NAME_RESULT_ITEMW[]> rItems;
        public void EncodeDeferrals(Titanis.DceRpc.IRpcEncoder encoder) {
            if ((null != this.rItems)) {
                encoder.WriteArrayHeader<DS_NAME_RESULT_ITEMW>(this.rItems.value);
                for (int i = 0; (i < this.rItems.value.Length); i++) {
                    encoder.WriteFixedStruct(this.rItems.value[i], Titanis.DceRpc.NdrAlignment._4Byte);
                }
                for (int i = 0; (i < this.rItems.value.Length); i++) {
                    encoder.WriteStructDeferral(this.rItems.value[i]);
                }
            }
        }
        public void DecodeDeferrals(Titanis.DceRpc.IRpcDecoder decoder) {
            if ((null != this.rItems)) {
                this.rItems.value = decoder.ReadArrayHeader<DS_NAME_RESULT_ITEMW>();
                for (int i = 0; (i < this.rItems.value.Length); i++) {
                    this.rItems.value[i] = decoder.ReadFixedStruct<DS_NAME_RESULT_ITEMW>(Titanis.DceRpc.NdrAlignment._4Byte);
                }
                for (int i = 0; (i < this.rItems.value.Length); i++) {
                    decoder.ReadStructDeferral<DS_NAME_RESULT_ITEMW>(ref this.rItems.value[i]);
                }
            }
        }
    }

    /// <summary>
    /// DRSCrackNames reply version 1. [MS-DRSR] § 4.1.4.1.5
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Animus IDL Compiler", "0.9.4")]
    public struct DRS_MSG_CRACKREPLY_V1 : Titanis.DceRpc.IRpcFixedStruct {
        public void Encode(Titanis.DceRpc.IRpcEncoder encoder) {
            encoder.WritePointer(this.pResult);
        }
        public void Decode(Titanis.DceRpc.IRpcDecoder decoder) {
            this.pResult = decoder.ReadPointer<DS_NAME_RESULTW>();
        }
        public RpcPointer<DS_NAME_RESULTW> pResult;
        public void EncodeDeferrals(Titanis.DceRpc.IRpcEncoder encoder) {
            if ((null != this.pResult)) {
                encoder.WriteFixedStruct(this.pResult.value, Titanis.DceRpc.NdrAlignment._4Byte);
                encoder.WriteStructDeferral(this.pResult.value);
            }
        }
        public void DecodeDeferrals(Titanis.DceRpc.IRpcDecoder decoder) {
            if ((null != this.pResult)) {
                this.pResult.value = decoder.ReadFixedStruct<DS_NAME_RESULTW>(Titanis.DceRpc.NdrAlignment._4Byte);
                decoder.ReadStructDeferral<DS_NAME_RESULTW>(ref this.pResult.value);
            }
        }
    }

    // -------------------------------------------------------------------------
    // drsuapi interface marker
    // -------------------------------------------------------------------------

    public interface drsuapi {
    }

    // -------------------------------------------------------------------------
    // Client proxy
    // -------------------------------------------------------------------------

    [System.CodeDom.Compiler.GeneratedCodeAttribute("Animus IDL Compiler", "0.9.4")]
    public class drsClientProxy : Titanis.DceRpc.Client.RpcClientProxy, drsuapi, Titanis.DceRpc.IRpcClientProxy {
        private static System.Guid _interfaceUuid = new System.Guid("e3514235-4b06-11d1-ab04-00c04fc2dcd2");

        public override System.Guid InterfaceUuid {
            get { return _interfaceUuid; }
        }
        public override Titanis.DceRpc.RpcVersion InterfaceVersion {
            get { return new Titanis.DceRpc.RpcVersion(4, 0); }
        }

        /// <summary>
        /// IDL_DRSBind (opnum 0). [MS-DRSR] § 4.1.3
        /// </summary>
        public virtual async Task<uint> IDL_DRSBind(
            RpcPointer<ms_dtyp.GUID> puuidClientDsa,
            RpcPointer<DRS_EXTENSIONS> pextClient,
            RpcPointer<RpcPointer<DRS_EXTENSIONS>> ppextServer,
            RpcPointer<Titanis.DceRpc.RpcContextHandle> phDrs,
            System.Threading.CancellationToken cancellationToken)
        {
            Titanis.DceRpc.Client.IRpcRequestBuilder req = this.CreateRequest(0);
            Titanis.DceRpc.IRpcEncoder encoder = req.StubData;
            encoder.WritePointer(puuidClientDsa);
            if ((null != puuidClientDsa)) {
                encoder.WriteFixedStruct(puuidClientDsa.value, Titanis.DceRpc.NdrAlignment._4Byte);
            }
            encoder.WritePointer(pextClient);
            if ((null != pextClient)) {
                encoder.WriteConformantStruct(pextClient.value, Titanis.DceRpc.NdrAlignment._4Byte);
                encoder.WriteStructDeferral(pextClient.value);
            }
            var sendTask = this.SendRequestAsync(req, cancellationToken);
            Titanis.DceRpc.IRpcDecoder decoder = await sendTask;
            ppextServer.value = decoder.ReadOutPointer<DRS_EXTENSIONS>(ppextServer.value);
            if ((null != ppextServer.value)) {
                ppextServer.value.value = decoder.ReadConformantStruct<DRS_EXTENSIONS>(Titanis.DceRpc.NdrAlignment._4Byte);
                decoder.ReadStructDeferral<DRS_EXTENSIONS>(ref ppextServer.value.value);
            }
            phDrs.value = decoder.ReadContextHandle();
            uint retval = decoder.ReadUInt32();
            return retval;
        }

        /// <summary>
        /// IDL_DRSUnbind (opnum 1). [MS-DRSR] § 4.1.26
        /// </summary>
        public virtual async Task<uint> IDL_DRSUnbind(
            RpcPointer<Titanis.DceRpc.RpcContextHandle> phDrs,
            System.Threading.CancellationToken cancellationToken)
        {
            Titanis.DceRpc.Client.IRpcRequestBuilder req = this.CreateRequest(1);
            Titanis.DceRpc.IRpcEncoder encoder = req.StubData;
            encoder.WriteContextHandle(phDrs.value);
            var sendTask = this.SendRequestAsync(req, cancellationToken);
            Titanis.DceRpc.IRpcDecoder decoder = await sendTask;
            phDrs.value = decoder.ReadContextHandle();
            uint retval = decoder.ReadUInt32();
            return retval;
        }

        /// <summary>
        /// IDL_DRSGetNCChanges (opnum 3). [MS-DRSR] § 4.1.10
        /// Sends dwInVersion=8, expects dwOutVersion=6.
        /// </summary>
        public virtual async Task<uint> IDL_DRSGetNCChanges(
            Titanis.DceRpc.RpcContextHandle hDrs,
            DRS_MSG_GETCHGREQ_V8 pmsgIn,
            RpcPointer<uint> pdwOutVersion,
            RpcPointer<DRS_MSG_GETCHGREPLY_V6> pmsgOut,
            System.Threading.CancellationToken cancellationToken)
        {
            Titanis.DceRpc.Client.IRpcRequestBuilder req = this.CreateRequest(3);
            Titanis.DceRpc.IRpcEncoder encoder = req.StubData;
            encoder.WriteContextHandle(hDrs);
            encoder.WriteValue((uint)8);  // dwInVersion = 8
            encoder.WriteFixedStruct(pmsgIn, Titanis.DceRpc.NdrAlignment._4Byte);
            encoder.WriteStructDeferral(pmsgIn);
            var sendTask = this.SendRequestAsync(req, cancellationToken);
            Titanis.DceRpc.IRpcDecoder decoder = await sendTask;
            pdwOutVersion.value = decoder.ReadUInt32();
            pmsgOut.value = decoder.ReadFixedStruct<DRS_MSG_GETCHGREPLY_V6>(Titanis.DceRpc.NdrAlignment._4Byte);
            decoder.ReadStructDeferral<DRS_MSG_GETCHGREPLY_V6>(ref pmsgOut.value);
            uint retval = decoder.ReadUInt32();
            return retval;
        }

        /// <summary>
        /// IDL_DRSCrackNames (opnum 12). [MS-DRSR] § 4.1.4
        /// Sends dwInVersion=1, expects dwOutVersion=1.
        /// </summary>
        public virtual async Task<uint> IDL_DRSCrackNames(
            Titanis.DceRpc.RpcContextHandle hDrs,
            DRS_MSG_CRACKREQ_V1 pmsgIn,
            RpcPointer<uint> pdwOutVersion,
            RpcPointer<DRS_MSG_CRACKREPLY_V1> pmsgOut,
            System.Threading.CancellationToken cancellationToken)
        {
            Titanis.DceRpc.Client.IRpcRequestBuilder req = this.CreateRequest(12);
            Titanis.DceRpc.IRpcEncoder encoder = req.StubData;
            encoder.WriteContextHandle(hDrs);
            encoder.WriteValue((uint)1);  // dwInVersion = 1
            encoder.WriteFixedStruct(pmsgIn, Titanis.DceRpc.NdrAlignment._4Byte);
            encoder.WriteStructDeferral(pmsgIn);
            var sendTask = this.SendRequestAsync(req, cancellationToken);
            Titanis.DceRpc.IRpcDecoder decoder = await sendTask;
            pdwOutVersion.value = decoder.ReadUInt32();
            pmsgOut.value = decoder.ReadFixedStruct<DRS_MSG_CRACKREPLY_V1>(Titanis.DceRpc.NdrAlignment._4Byte);
            decoder.ReadStructDeferral<DRS_MSG_CRACKREPLY_V1>(ref pmsgOut.value);
            uint retval = decoder.ReadUInt32();
            return retval;
        }
    }
}
