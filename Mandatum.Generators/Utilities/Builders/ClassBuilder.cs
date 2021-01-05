using System;

namespace Mandatum.Generators.Utilities.Builders
{
	public class ClassBuilder : Builder
	{
		public string Name { get; }
		public string Namespace { get; }

		public ClassBuilder(string name,
			string @namespace,
			Accessibility accessibility,
			string[] namespaces = null) : base(0)
		{
			Name = name;
			Namespace = @namespace;

			Append($"namespace {Namespace} {{");
			Padding += 1;
			AppendLine();

			if (namespaces is not null)
			{
				foreach (var namespaceToImport in namespaces)
				{
					AppendLine($"using {namespaceToImport};");
				}
			}

			var accessiblityString = accessibility.ToKeyword();

			AppendLine($"{accessibility} class {name}");
			AppendLine("{");
			Padding += 1;
		}

		public void AddField(string name,
			string type,
			Accessibility accessibility,
			GetterType getter = GetterType.None,
			SetterType setter = SetterType.None)
		{
			var ending = "";

			if (getter != GetterType.None && setter != SetterType.None)
			{
				var getterString = getter.GetKeyword();
				var setterString = setter.GetKeyword();

				ending = $" {{{setterString} {getterString}}}";
			}
			else
			{
				ending = ";";
			}


			AppendLine($"{accessibility.ToKeyword()} {type} {name}{ending}");
		}

		public void AddMethod(string name,
			string returnType,
			Accessibility accessibility,
			BlockBuilder content,
			string[] parameters = null)
		{
			var parameterString = parameters is null ? "" : string.Join(",", parameters);

			AppendLine($"{accessibility.ToKeyword()} {returnType} {name}({parameterString})");

			AppendLine(content.ToString());

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