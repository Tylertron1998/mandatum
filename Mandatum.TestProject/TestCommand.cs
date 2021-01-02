using System;
using System.Net;
using System.Threading.Tasks;
using Mandatum.Commands;

namespace Mandatum.TestProject
{
	[Delimiter(",")]
	public class TestCommand : ICommand<int, HttpStatusCode>
	{
		public async Task RunAsync(int param0, HttpStatusCode code)
		{
			Console.WriteLine($"I got {param0} and a status code of {(int) code}");
		}
	}
}