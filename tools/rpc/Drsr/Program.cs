using System.ComponentModel;
using Titanis.Cli;

namespace Drsr;

[Description("Commands for interacting with the Directory Replication Service")]
[Subcommand("dcsync", typeof(DcSyncCommand))]
internal class Program : MultiCommand
{
	static int Main(string[] args)
		=> RunProgramAsync<Program>(args);
}
