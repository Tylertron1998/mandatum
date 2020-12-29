using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Mandatum.Generators.SyntaxReceivers
{
	public class CommandSyntaxReceiver : ISyntaxReceiver
	{
		private List<ClassDeclarationSyntax> _classes = new List<ClassDeclarationSyntax>();
		private List<InterfaceDeclarationSyntax> _interfaces = new List<InterfaceDeclarationSyntax>();
		public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
		{
			if (syntaxNode is ClassDeclarationSyntax @class)
			{
				_classes.Add(@class);
			}
			if (syntaxNode is InterfaceDeclarationSyntax @interface)
			{
				_interfaces.Add(@interface);
			}
		}

		public IEnumerable<INamedTypeSymbol> GetAllCommandsImplementingInterface(Compilation currentCompilation)
		{
			var commands = new List<INamedTypeSymbol>();

			foreach(var @class in _classes)
			{
				var semanticModel = currentCompilation.GetSemanticModel(@class.SyntaxTree);
				var declaredSymbol = ModelExtensions.GetDeclaredSymbol(semanticModel, @class) as INamedTypeSymbol;

				var interfaces = declaredSymbol.Interfaces;

				var commandInterfaces = GetCommandInterfaces(currentCompilation);

				if (declaredSymbol.Interfaces.Any(x =>
				{
					return commandInterfaces.Any(@interface =>
						SymbolEqualityComparer.Default.Equals(x.ConstructedFrom, @interface));
				}))
				{
					commands.Add(declaredSymbol);
				}
				
				//
				// commands.AddRange(interfaces.Where(x =>
				// {
				// 	bool equals = false;
				// 	foreach (var commandInterface in GetCommandInterfaces(currentCompilation))
				// 	{
				// 		equals = SymbolEqualityComparer.Default.Equals(x.ConstructedFrom, commandInterface);
				// 		if (equals)
				// 		{
				// 			ifc = x;
				// 			break;
				// 		}
				// 	}
				//
				// 	return equals;
				// }));

			}

			return commands;
		}
		
		private IEnumerable<INamedTypeSymbol> GetCommandInterfaces(Compilation compilation)
		{
			var commandInterfaces = new List<INamedTypeSymbol>();

			foreach (var @interface in _interfaces)
			{
				var semanticModel = compilation.GetSemanticModel(@interface.SyntaxTree);
				var symbol = ModelExtensions.GetDeclaredSymbol(semanticModel, @interface) as INamedTypeSymbol;

				if (symbol.Name.StartsWith("ICommand") && symbol.IsGenericType &&
				    symbol.ContainingNamespace.ToDisplayString() == "Mandatum.Interfaces")
				{
					commandInterfaces.Add(symbol);
				}
			}

			return commandInterfaces;
		}
	}
}