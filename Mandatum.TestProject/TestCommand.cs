using System.Threading.Tasks;
using Mandatum.Interfaces;

namespace Mandatum.TestProject
{
	public class TestCommand : ICommand<int>
	{
		public async Task RunAsync(int param0)
		{
		}
	}
}