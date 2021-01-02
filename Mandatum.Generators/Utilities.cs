using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Mandatum.Generators
{
	public static class Utilities
	{
		public static string BuildCommandInterfaces(uint maximumCommandParams)
		{
			var sb = new StringBuilder();
			sb.AppendLine("using System.Threading.Tasks;");
			sb.AppendLine();
			sb.AppendLine("namespace Mandatum.Commands");
			sb.AppendLine("{");

			for (var i = 0; i < maximumCommandParams; i++)
			{
				var name = "ICommand<";
                
				var signature = "";

				for (var x = 0; x <= i; x++)
				{
					if (x > 0) signature += ", ";
					signature += $"T{x}";
				}

				name += signature;
				name += ">";

				var paramlist = string.Join(",", signature.Split(',').Select(x => x += $" {x.ToUpper()}"));
                
				sb.AppendLine($"public interface {name}");
				sb.AppendLine("{");
				sb.AppendLine($"public Task RunAsync({paramlist});");
				sb.AppendLine("}");
			}

			sb.AppendLine("}");
			return sb.ToString();
		}
		
		public static string GetValueOrDefault(this AnalyzerConfigOptions options, string key, string defaultValue)
		{
			if (options.TryGetValue(key, out var value)) return value;
			return defaultValue;
		}

		public static void AddIfNotExists<T>(this HashSet<T> hashSet, T value)
		{
			if (hashSet.Contains(value)) return;
			hashSet.Add(value);
		}
	}
}