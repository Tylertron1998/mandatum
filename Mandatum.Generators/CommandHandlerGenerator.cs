using System.Diagnostics;
using Mandatum.Generators.SyntaxReceivers;
using Microsoft.CodeAnalysis;

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
			var value = ParseOptions(context);

			var commandInterfacesSource = Utilities.BuildCommandInterfaces(value);
			
			context.AddSource("Mandatum.Interfaces.cs", commandInterfacesSource);

		}

		private static uint ParseOptions(GeneratorExecutionContext context)
		{
			var options = context.AnalyzerConfigOptions.GlobalOptions;

			var givenKey = options.GetValueOrDefault("maximum_command_arguments", "5");

			if (!uint.TryParse(givenKey,
				out var value))
			{
				var descripter = new DiagnosticDescriptor(
					"MANDATUM001",
					"Couldn't parse 'maximum_command_arguments' parameter",
					"Couldn't parse 'maximum_command_arguments' parameter due to invalid integer passed, got: {0}",
					"Mandatum",
					DiagnosticSeverity.Error,
					true);

				context.ReportDiagnostic(Diagnostic.Create(descripter, Location.None, givenKey));
			}

			return value;
		}
	}
}