using System.Diagnostics;
using System.Linq;
using Mandatum.Generators.SyntaxReceivers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Mandatum.Generators
{
	[Generator]
	public class CommandHandlerGenerator : ISourceGenerator
	{
		public void Initialize(GeneratorInitializationContext context)
		{
			context.RegisterForSyntaxNotifications(() => new CommandSyntaxReceiver());
		}

		public void Execute(GeneratorExecutionContext context)
		{
			var options = ParseOptions(context);

			var commandInterfacesSource = Utilities.BuildCommandInterfaces(options.MaximumCommandArguments);
			
			context.AddSource("Mandatum.CommandInterfaces.g.cs", commandInterfacesSource);

			var interfaceSourceSyntaxTree = SyntaxFactory.ParseSyntaxTree(commandInterfacesSource);

			var compilation = context.Compilation.AddSyntaxTrees(interfaceSourceSyntaxTree);
			
			var reciever = context.SyntaxReceiver as CommandSyntaxReceiver;

			foreach (var node in interfaceSourceSyntaxTree.GetRoot().DescendantNodes())
			{
				reciever!.OnVisitSyntaxNode(node);
			}

			var symbols = reciever!.GetAllCommandsImplementingInterface(compilation);

			foreach (var symbol in symbols)
			{
				var descripter = new DiagnosticDescriptor(
					"MANDATUM002",
					"TESTING IF THIS WORKS",
					"found command named: {0} with arguments: {1}",
					"Mandatum",
					DiagnosticSeverity.Warning,
					true);

				var commandInterface = symbol.Interfaces.First(typeSymbol => typeSymbol.Name == "ICommand");

				var arguments = string.Join(',', commandInterface.TypeArguments.Select(argument => argument.ToDisplayString()));
				
				context.ReportDiagnostic(Diagnostic.Create(descripter, Location.None, symbol.Name, arguments, symbol.Kind));
			}

		}

		private static Options ParseOptions(GeneratorExecutionContext context)
		{
			var options = context.AnalyzerConfigOptions.GlobalOptions;

			var givenKey = options.GetValueOrDefault("maximum_command_arguments", "5");

			if (!uint.TryParse(givenKey,
				out var value))
			{
				var descripter = new DiagnosticDescriptor(
					"MANDATUM001",
					"Couldn't parse 'maximum_command_arguments' parameter",
					"Couldn't parse 'maximum_command_arguments' parameter due to invalid unsigned integer passed, got: {0}",
					"Mandatum",
					DiagnosticSeverity.Error,
					true);

				context.ReportDiagnostic(Diagnostic.Create(descripter, Location.None, givenKey));
			}

			return new Options(value);
		}
	}
}