using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Mandatum.Generators.Objects;
using Mandatum.Generators.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Formatter = Microsoft.CodeAnalysis.Formatting.Formatter;

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

			// create the text based source code for ICommand<T...>
			var commandInterfacesSource = Utilities.BuildCommandInterfaces(options.MaximumCommandArguments);
			
			// add the source to the compilation
			context.AddSource("Mandatum.Commands.cs", commandInterfacesSource);

			// create a syntax tree for it
			var interfaceSourceSyntaxTree = SyntaxFactory.ParseSyntaxTree(commandInterfacesSource);

			// create a new compilation that has those syntax trees.
			var compilation = context.Compilation.AddSyntaxTrees(interfaceSourceSyntaxTree);
			
			var reciever = context.SyntaxReceiver as CommandSyntaxReceiver;

			var syntaxFinder = new SyntaxFinder(compilation);


			var commandSymbols = syntaxFinder.GetCommandSymbols(reciever.Classes, reciever.Interfaces);

			var userResolvers = syntaxFinder.GetUserResolverSymbols(reciever.Classes);
			
			var (resolverInfo, commandInfo) = BuildInformationClasses(userResolvers, commandSymbols);
			
			var commandHandlerSource = BuildCommandHandlerSource(resolverInfo.ToArray(), commandInfo);

			File.WriteAllText(@"C:\Users\sirbr\Documents\coding\projects\C#\mandatum\Mandatum.Generators\obj\GeneratedFiles\CommandHandler.cs", commandHandlerSource);
			
			context.AddSource("Mandatum.CommandHandler.cs", commandHandlerSource);

		}

		private (IEnumerable<ResolverDeclarationInfo>, IEnumerable<CommandDeclarationInfo>) BuildInformationClasses(IEnumerable<INamedTypeSymbol> resolvers, IEnumerable<INamedTypeSymbol> commandSymbols)
		{
			return (
				resolvers.Select(resolver => new ResolverDeclarationInfo
			{
				Name = resolver.Name,
				ContainingNamespace = resolver.ContainingNamespace.ToDisplayString(),
				ConversionType = resolver.Interfaces.First(x => x.Name.StartsWith("IResolver")).TypeArguments.First(),
				IsAsync = resolver.Interfaces.First().Name.StartsWith("IAsync")
			}), 
				commandSymbols.Select(command => new CommandDeclarationInfo()
				{
					Name = command.Name,
					Arguments = command.Interfaces.First(x => x.Name == "ICommand").TypeArguments,
					ContainingNamespace = command.ContainingNamespace.ToDisplayString(),
					Delimiter = command.GetAttributes().First(x => x.AttributeClass.Name == "DelimiterAttribute").ConstructorArguments.First().Value.ToString()
				})	
				);
		}

		private string BuildCommandHandlerSource(ResolverDeclarationInfo[] resolvers, IEnumerable<CommandDeclarationInfo> commands)
		{
			var commandHandlerClassBuilder = new StringBuilder();
			var ctorBuilder = new StringBuilder();
			var namespaceBuilder = new StringBuilder();
			var runMethodBuilder = new StringBuilder();
			
			// TODO: DI for commands
			
			var currentNamespaces = new HashSet<string>();
			var fields = new List<string>();
			
			fields.Add("private CommandExecutionOptions _options;");

			//ctor

			currentNamespaces.AddIfNotExists("Mandatum");

			ctorBuilder.Append("public CommandHandler(CommandExecutionOptions options)");
			ctorBuilder.AppendLine("{");
			ctorBuilder.AppendLine("_options = options;");
			
			//run method
			currentNamespaces.AddIfNotExists("Mandatum.Test");
			runMethodBuilder.Append("public async Task RunAsync(Message message)");
			runMethodBuilder.AppendLine("{");
			runMethodBuilder.AppendLine("if(!message.Content.StartsWith(_options.Prefix)) return;");
			runMethodBuilder.AppendLine("var prefixless = message.Content.Substring(_options.Prefix.Length);");
			runMethodBuilder.AppendLine("var commandName = prefixless.Substring(0, prefixless.IndexOf(\" \"));");
			runMethodBuilder.AppendLine("var rest = prefixless.Substring(prefixless.IndexOf(\" \"));");

			runMethodBuilder.AppendLine("var t = commandName switch {");

			var localMethodsBuilder = new StringBuilder();

			currentNamespaces.AddIfNotExists("System.Threading.Tasks");
			currentNamespaces.AddIfNotExists("System");
			
			foreach (var command in commands)
			{
				currentNamespaces.AddIfNotExists(command.ContainingNamespace);
				
				ctorBuilder.AppendLine($"_{command.Name} = new {command.Name}();");

				fields.Add($"private {command.Name} _{command.Name};");

				runMethodBuilder.AppendLine($"\"{command.Name}\" => Execute{command.Name}(rest),");

				localMethodsBuilder.AppendLine($"Task Execute{command.Name}(string args)");
				localMethodsBuilder.AppendLine("{");
				localMethodsBuilder.AppendLine($"var split = args.Split(\"{command.Delimiter}\");");
				localMethodsBuilder.AppendLine($"if(split.Length < {command.Arguments.Count()}) throw new Exception(\"Incorrect args\");");

				var index = 0;

				var arglist = new List<string>();
				
				foreach (var argument in command.Arguments)
				{
					var argumentResolver = resolvers.First(func);

					bool func(ResolverDeclarationInfo resolver)
					{
						var isEqual = SymbolEqualityComparer.Default.Equals(resolver.ConversionType, argument);
						return isEqual;
					}

					var async = argumentResolver.IsAsync;
					localMethodsBuilder.AppendLine($"var arg{index} = {(async ? "await" : "")} _{argumentResolver.Name}.Resolve{(async ? "Async" : "")}(split[{index}]);");

					arglist.Add($"arg{index}");
					
					index++;
				}

				localMethodsBuilder.AppendLine($"return _{command.Name}.RunAsync({string.Join(", ", arglist)});");
				localMethodsBuilder.AppendLine("}");

				runMethodBuilder.AppendLine("_ => throw new Exception(\"unknown command\")");
				runMethodBuilder.AppendLine("};");

			}

			foreach (var resolver in resolvers)
			{
				currentNamespaces.AddIfNotExists(resolver.ContainingNamespace);

				ctorBuilder.AppendLine($"_{resolver.Name} = new {resolver.Name}();");

				fields.Add($"private {resolver.Name} _{resolver.Name};");
			}

			runMethodBuilder.AppendLine(localMethodsBuilder.ToString());
			runMethodBuilder.AppendLine("}");

			ctorBuilder.AppendLine("}");
			
			// namespaces

			foreach (var @namespace in currentNamespaces)
			{
				namespaceBuilder.AppendLine($"using {@namespace};");
			}
			
			// actual command handler class

			commandHandlerClassBuilder.Append(namespaceBuilder);
			commandHandlerClassBuilder.AppendLine("namespace Mandatum.Commands");
			commandHandlerClassBuilder.AppendLine("{");
			commandHandlerClassBuilder.AppendLine("public class CommandHandler");
			commandHandlerClassBuilder.AppendLine("{");

			foreach (var field in fields)
			{
				commandHandlerClassBuilder.AppendLine(field);
			}
			
			commandHandlerClassBuilder.AppendLine(ctorBuilder.ToString());

			commandHandlerClassBuilder.AppendLine(runMethodBuilder.ToString());

			commandHandlerClassBuilder.AppendLine("}");
			commandHandlerClassBuilder.AppendLine("}");
			
			return commandHandlerClassBuilder.ToString();
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