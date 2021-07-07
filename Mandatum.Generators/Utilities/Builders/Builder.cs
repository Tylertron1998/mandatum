using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Mandatum.Generators.Utilities.Builders
{
	[DebuggerDisplay("{ToDebuggerString()}")]
	public abstract class Builder
	{
		public ushort Padding;
		protected readonly StringBuilder StringBuilder;

		protected Builder(ushort padding)
		{
			Padding = padding;
			StringBuilder = new StringBuilder();
		}

		public void Append(string content, bool pad = true)
		{
			var padding = pad ? GetPadding() : "";
			StringBuilder.Append($"{padding}{content}");
		}

		public BlockBuilder GetBuilder()
		{
			return new BlockBuilder(Padding);
		}

		public void AppendLine(string content = "", bool pad = true)
		{
			if (content == "")
			{
				StringBuilder.AppendLine();
			}
			else
			{
				var padding = pad ? GetPadding() : "";
				StringBuilder.AppendLine($"{padding}{content}");
			}
		}

		public override string ToString()
		{
			return StringBuilder.ToString();
		}

		private string ToDebuggerString() => StringBuilder.ToString();
		

		private string GetPadding()
		{
			var padding = string.Join("", Enumerable.Repeat("	", Padding));
			return padding;
		}
	}
}