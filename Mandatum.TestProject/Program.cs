using System.Threading.Tasks;
using Mandatum.Commands;
using Mandatum.Test;

namespace Mandatum.TestProject
{
	public static class Program
	{
		public static async Task Main(string[] args)
		{
			var options = new CommandExecutionOptions
			{
				Prefix = "t!"
			};

			var message = new Message("t!TestCommand a, https://httpstat.us/404");
			var handler = new CommandHandler(options);
			await handler.RunAsync(message);
		}
	}
}