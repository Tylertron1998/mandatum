namespace Mandatum.Resolvers
{
	public class Int32Resolver : IResolver<int>
	{
		// TODO: this needs to be emitted, otherwise we can't pick it up via the scanner.
		// TODO: use some Result<T> type, for this and other places (i.e. command handler)
		public int Resolve(string argument)
		{
			return int.Parse(argument);
		}
	}
}