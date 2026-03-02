using System;
using System.Threading;
using System.Threading.Tasks;
using Titanis.DceRpc;

namespace Titanis.Msrpc.Msdrsr
{
	/// <summary>
	/// Base class for DRS objects that hold a server-side context handle.
	/// </summary>
	public abstract class DrsObject : IDisposable
	{
		protected readonly DrsClient _client;
		protected readonly RpcContextHandle _handle;

		private protected DrsObject(DrsClient client, RpcContextHandle handle)
		{
			this._client = client;
			this._handle = handle;
		}

		protected abstract Task CloseAsync(CancellationToken cancellationToken);

		#region Dispose pattern
		private bool _isDisposed;

		protected virtual void Dispose(bool disposing)
		{
			if (!_isDisposed)
			{
				if (disposing)
				{
					_ = this.CloseAsync(CancellationToken.None);
				}
				_isDisposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
