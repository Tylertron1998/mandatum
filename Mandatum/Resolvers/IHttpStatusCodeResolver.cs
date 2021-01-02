using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Mandatum.Resolvers
{
	public class IHttpStatusCodeResolver : IAsyncResolver<HttpStatusCode>
	{
		public async Task<HttpStatusCode> ResolveAsync(string message)
		{
			var client = new HttpClient();
			var result = await client.GetAsync(message);
			return result.StatusCode;
		}
	}
}