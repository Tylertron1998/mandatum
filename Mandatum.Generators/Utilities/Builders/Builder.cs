using System.Linq;
using System.Text;

namespace Mandatum.Generators.Utilities.Builders
{
	public abstract class Builder
	{
		protected ushort Padding;
		private readonly StringBuilder _builder;

		protected Builder(ushort padding)
		{
			Padding = padding;
			_builder = new StringBuilder();
		}

		protected void Append(string content)
		{
			var padding = GetPadding();
			_builder.Append($"{padding}{content}");
		}

		public BlockBuilder GetBuilder()
		{
			return new BlockBuilder(Padding);
		}

		protected void AppendLine(string content = "")
		{
			if (content == "")
			{
				_builder.AppendLine();
			}
			else
			{
				var padding = GetPadding();
				_builder.AppendLine($"{padding}{content}");
			}
		}

		public override string ToString()
		{
			return _builder.ToString();
		}

		private string GetPadding()
		{
			var padding = string.Join("", Enumerable.Repeat("	", Padding));
			return padding;
		}
	}
}