using ms_drsr;
using ms_dtyp;
using System;

namespace Titanis.Msrpc.Msdrsr
{
	public static class RpcStructExtensions
	{
		/// <summary>
		/// Converts a .NET <see cref="Guid"/> to the NDR <see cref="ms_dtyp.GUID"/> wire format.
		/// </summary>
		public static ms_dtyp.GUID ToRpcGuid(this Guid guid)
		{
			// Guid.ToByteArray() uses mixed-endian encoding.
			// ms_dtyp.GUID: Data1=uint, Data2=ushort, Data3=ushort, Data4=byte[8]
			byte[] b = guid.ToByteArray();
			return new ms_dtyp.GUID
			{
				Data1 = BitConverter.ToUInt32(b, 0),
				Data2 = BitConverter.ToUInt16(b, 4),
				Data3 = BitConverter.ToUInt16(b, 6),
				Data4 = new byte[] { b[8], b[9], b[10], b[11], b[12], b[13], b[14], b[15] },
			};
		}

		/// <summary>
		/// Converts an NDR <see cref="ms_dtyp.GUID"/> back to a .NET <see cref="Guid"/>.
		/// </summary>
		public static Guid ToGuid(this in ms_dtyp.GUID rpcGuid)
		{
			byte[] b = new byte[16];
			BitConverter.TryWriteBytes(b.AsSpan(0, 4), rpcGuid.Data1);
			BitConverter.TryWriteBytes(b.AsSpan(4, 2), rpcGuid.Data2);
			BitConverter.TryWriteBytes(b.AsSpan(6, 2), rpcGuid.Data3);
			if (rpcGuid.Data4 != null)
				Array.Copy(rpcGuid.Data4, 0, b, 8, Math.Min(8, rpcGuid.Data4.Length));
			return new Guid(b);
		}

		/// <summary>
		/// Gets the string name from a <see cref="DSNAME"/>, trimming the null terminator.
		/// </summary>
		public static string? GetName(this in DSNAME dsname)
		{
			if (dsname.StringName == null || dsname.NameLen == 0)
				return null;
			return new string(dsname.StringName, 0, (int)dsname.NameLen);
		}

		/// <summary>
		/// Returns true if a name translation result item succeeded.
		/// </summary>
		public static bool IsSuccess(this in DS_NAME_RESULT_ITEMW item)
			=> item.status == (uint)DS_NAME_ERROR.DS_NAME_NO_ERROR;
	}
}
