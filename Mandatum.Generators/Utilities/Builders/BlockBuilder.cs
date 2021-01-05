namespace Mandatum.Generators.Utilities.Builders
{
	public class BlockBuilder : Builder
	{
		public BlockBuilder(ushort padding) : base((ushort)(padding + 1))
		{
			Append("{");
			Padding += 1;
		}

		public override string ToString()
		{
			Padding -= 1;
			Append("}");
			return base.ToString();
		}
	}
}