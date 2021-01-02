using System.Collections.Generic;
using System.Collections.Immutable;
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
			}

			return commands;
		}

		public IEnumerable<INamedTypeSymbol> GetSyncResolvers(Compilation currentCompilation)
		{

			// TODO: this is a complete mess
			// TODO: add a class for this like we have for CommandDeclarationInfo

			var resolvers = new List<INamedTypeSymbol>();
			var intResolver = currentCompilation.GetTypeByMetadataName("Mandatum.Resolvers.Int32Resolver");
			
			resolvers.Add(intResolver); 
			// TODO: this whole class is a mess. All of this logic should be extracted, and this should be handled by something else.
			
			foreach(var @class in _classes)
			{
				var semanticModel = currentCompilation.GetSemanticModel(@class.SyntaxTree);
				var declaredSymbol = ModelExtensions.GetDeclaredSymbol(semanticModel, @class) as INamedTypeSymbol;

				var interfaces = declaredSymbol.Interfaces;

				foreach (var @interface in interfaces)
				{
					var members = @interface.GetMembers();
					var methods = members.OfType<IMethodSymbol>();
					if (methods.Any(method => method.Name == "Resolve" && EqualParams(method.Parameters, @interface.TypeParameters)))
					{
						resolvers.Add(declaredSymbol);
					}
				}
				
				bool EqualParams(ImmutableArray<IParameterSymbol> methodParameters, ImmutableArray<ITypeParameterSymbol> interfaceTypeParameters)
				{
					if (methodParameters.Length != interfaceTypeParameters.Length) return false;

					for (var index = 0; index < methodParameters.Length; index++)
					{
						var methodParameter = methodParameters[index];
						var interfaceGenericParameter = interfaceTypeParameters[index];

						if (SymbolEqualityComparer.Default.Equals(methodParameter, interfaceGenericParameter))
						{
							return false;
						}

					}

					return true;
				}
				
			}

			return resolvers;
		}

		private IEnumerable<INamedTypeSymbol> GetCommandInterfaces(Compilation compilation)
		{
			var commandInterfaces = new List<INamedTypeSymbol>();

			foreach (var @interface in _interfaces)
			{
				var semanticModel = compilation.GetSemanticModel(@interface.SyntaxTree);
				var symbol = ModelExtensions.GetDeclaredSymbol(semanticModel, @interface) as INamedTypeSymbol;

				if (symbol.Name.StartsWith("ICommand") && symbol.IsGenericType &&
				    symbol.ContainingNamespace.ToDisplayString() == "Mandatum.Commands")
				{
					commandInterfaces.Add(symbol);
				}
			}

			return commandInterfaces;
		}
	}
}