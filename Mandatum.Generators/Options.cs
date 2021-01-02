
namespace Mandatum.Generators
{
	public struct Options
	{
		public uint MaximumCommandArguments { get; set; }

		public Options(uint maximumCommandArguments)
		{
			MaximumCommandArguments = maximumCommandArguments;
		}
	};
}