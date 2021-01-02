namespace Mandatum.Resolvers
{
	public interface IResolver<T>
	{
		T Resolve(string argument);
	}
}