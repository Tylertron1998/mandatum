using System.Diagnostics;

namespace Mandatum.Generators.Utilities.Builders
{
	[DebuggerDisplay("{ToDebuggerString()}")]
	public class ClassBuilder : Builder
	{
		public ClassBuilder(string name,
			string @namespace,
			Accessibility accessibility,
			string[] namespaces = null) : base(0)
		{
			Name = name;
			Namespace = @namespace;

			if (namespaces is not null)
			{
				foreach (var namespaceToImport in namespaces)
				{
					AppendLine($"using {namespaceToImport};");
				}
			}
			
			AppendLine();
			
			AppendLine($"namespace {Namespace}");
			AppendLine("{");
			Padding += 1;

			var accessiblityString = accessibility.ToKeyword();

			AppendLine($"{accessiblityString} class {name}");
			AppendLine("{");
			Padding += 1;
		}

		public string Name { get; }
		public string Namespace { get; }

		public void AddField(string name, string type, Accessibility accessibility)
		{
			AppendLine($"{accessibility.ToKeyword()} {type} {name};");
		}
		
		public void AddProperty(string name, string type, Accessibility accessibility, GetterType getter, SetterType setter)
		{
			var ending = $"{{ {getter.GetKeyword()} {setter.GetKeyword()} }}";
			AppendLine($"{accessibility.ToKeyword()} {type} {name} {ending}");
		}

		public void AddMethod(string name,
			string returnType,
			Accessibility accessibility,
			BlockBuilder content,
			string[] parameters = null)
		{
			var parameterString = parameters is null ? "" : string.Join(",", parameters);
			
			AppendLine($"{accessibility.ToKeyword()} {returnType} {name}({parameterString})");

			Append(content.ToString(), false);
		}

		public override string ToString()
		{
			Padding -= 1;
			AppendLine("}");
			Padding -= 1;
			AppendLine("}");
			return base.ToString();
		}
	}
}