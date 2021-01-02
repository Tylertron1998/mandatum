using System.Threading.Tasks;

namespace Mandatum.Resolvers
{
	public interface IAsyncResolver<T>
	{
		public Task<T> ResolveAsync(string message);
	}
}