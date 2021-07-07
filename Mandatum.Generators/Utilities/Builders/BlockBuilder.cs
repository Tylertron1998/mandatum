using System.Diagnostics;

namespace Mandatum.Generators.Utilities.Builders
{
	[DebuggerDisplay("{ToDebuggerString()}")]
	public class BlockBuilder : Builder
	{
		public BlockBuilder(ushort padding) : base(padding)
		{
			AppendLine("{");
			Padding += 1;
		}

		public override string ToString()
		{
			Padding -= 1;
			AppendLine("}");
			return base.ToString();
		}
	}
}