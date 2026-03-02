using ms_drsr;
using Titanis.Cli;
using Titanis.Msrpc.Msdrsr;

namespace Drsr;

/// <summary>
/// Base class for DRS commands.
/// </summary>
internal abstract class DrsCommand : RpcCommand<DrsClient>
{
	/// <inheritdoc/>
	protected sealed override Type InterfaceType => typeof(drsuapi);
}
