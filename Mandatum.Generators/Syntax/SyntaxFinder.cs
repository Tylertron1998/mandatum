using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Mandatum.Generators.Syntax
{
	public class SyntaxFinder
	{
		private Compilation _compilation;

		public SyntaxFinder(Compilation compilation)
		{
			_compilation = compilation;
		}
		
		/// <summary>
		/// This method is responsible for finding all the <see cref="INamedTypeSymbol"/>'s in the current compilation,
		/// that are considered a command - i.e. they implement one of the ICommand<T...> interfaces.
		/// </summary>
		/// <param name="classes">All <see cref="ClassDeclarationSyntax"/> in the current compilation.</param>
		/// <param name="interfaces"> All <see cref="InterfaceDeclarationSyntax"/> in the current compilation.</param>
		/// <returns>A <see cref="IEnumerable{INamedTypeSymbol}"/> of all class declarations that meet the requirements.</returns>
		public IEnumerable<INamedTypeSymbol> GetCommandSymbols(IEnumerable<ClassDeclarationSyntax> classes, IEnumerable<InterfaceDeclarationSyntax> interfaces)
		{
			var commands = new List<INamedTypeSymbol>();

			foreach(var @class in classes)
			{
				// we need the SemanticModel to look up the symbol
				var semanticModel = _compilation.GetSemanticModel(@class.SyntaxTree);
				// we then use that SemanticModel to find the INamedTypeSymbol for the class.
				var declaredSymbol = semanticModel.GetDeclaredSymbol(@class) as INamedTypeSymbol;

				// get all the interfaces we consider to be a ICommand<T...>
				var commandInterfaces = GetCommandInterfaces(interfaces);


				foreach (var @interface in commandInterfaces)
				{
					
					// do any of the interfaces on this class equal the interfaces we considered to be a ICommand<T...>?
					if (declaredSymbol.Interfaces.Any(commandInterface => SymbolEqualityComparer.Default.Equals(@interface)))
					{
						commands.Add(declaredSymbol); // if yes, add them.
					}
				}
			}

			return commands;
		}

		public IEnumerable<INamedTypeSymbol> GetUserResolvers(IEnumerable<ClassDeclarationSyntax> classes)
		{

			// TODO: this is a complete mess
			// TODO: add a class for this like we have for CommandDeclarationInfo

			var resolvers = new List<INamedTypeSymbol>();
			// TODO: this whole class is a mess. All of this logic should be extracted, and this should be handled by something else.
			
			foreach(var @class in classes)
			{
				var semanticModel = _compilation.GetSemanticModel(@class.SyntaxTree);
				var declaredSymbol = semanticModel.GetDeclaredSymbol(@class) as INamedTypeSymbol;

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

		private IEnumerable<INamedTypeSymbol> GetCommandInterfaces(IEnumerable<InterfaceDeclarationSyntax> interfaces)
		{
			var commandInterfaces = new List<INamedTypeSymbol>();

			foreach (var @interface in interfaces)
			{
				var semanticModel = _compilation.GetSemanticModel(@interface.SyntaxTree);
				var symbol = semanticModel.GetDeclaredSymbol(@interface) as INamedTypeSymbol;

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