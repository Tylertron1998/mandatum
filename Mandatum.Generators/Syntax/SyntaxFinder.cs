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
				var commandInterfaces = GetICommandInterfaces(interfaces);


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

		/// <summary>
		/// This method is used to lookup all user resolvers in the current compilation.
		/// </summary>
		/// <param name="classes">All classes present in the current compilation.</param>
		/// <returns>A <see cref="IEnumerable{INamedTypeSymbol}"/> with all current resolvers.</returns>
		public IEnumerable<INamedTypeSymbol> GetUserResolverSymbols(IEnumerable<ClassDeclarationSyntax> classes)
		{
			var resolvers = new List<INamedTypeSymbol>();

			foreach(var @class in classes)
			{
				// again we need the semantic model to extract type information.
				var semanticModel = _compilation.GetSemanticModel(@class.SyntaxTree);
				var declaredSymbol = semanticModel.GetDeclaredSymbol(@class) as INamedTypeSymbol;

				var interfaces = declaredSymbol.Interfaces;

				foreach (var @interface in interfaces)
				{
					var members = @interface.GetMembers();
					// we're getting all members that are of type IMethodSymbol (i.e. all methods)
					var methods = members.OfType<IMethodSymbol>();
					
					// if the method name is "Resolve" and the generic type parameters are equal to the method parameters, we can be
					// fairly sure that this is actually a Resolver.
					if (methods.Any(method => method.Name == "Resolve" && EqualParams(method.Parameters, @interface.TypeParameters)))
					{
						resolvers.Add(declaredSymbol);
					}
				}
				
				bool EqualParams(ImmutableArray<IParameterSymbol> methodParameters, ImmutableArray<ITypeParameterSymbol> interfaceTypeParameters)
				{
					// if they have two different lengths, or either one does not have exactly 1 parameter, then no, it's almost certainly not a resolver.
					if (methodParameters.Length != interfaceTypeParameters.Length || methodParameters.Length != 0) return false;

					var methodParameter = methodParameters.First();
					var interfaceGenericParameter = interfaceTypeParameters.First();

					return SymbolEqualityComparer.Default.Equals(methodParameter, interfaceGenericParameter);
				}
				
			}

			return resolvers;
		}

		/// <summary>
		/// This method is used for finding all interfaces we consider to be a ICommand<T...>.
		/// </summary>
		/// <param name="interfaces">All interfaces present in the current compilation.</param>
		/// <returns>A <see cref="IEnumerable{INamedTypeSymbol}"/> representing all ICommand<T...>'s in the current compilation.</returns>
		private IEnumerable<INamedTypeSymbol> GetICommandInterfaces(IEnumerable<InterfaceDeclarationSyntax> interfaces)
		{
			var commandInterfaces = new List<INamedTypeSymbol>();

			foreach (var @interface in interfaces)
			{
				// again we need the semantic model for type info.
				var semanticModel = _compilation.GetSemanticModel(@interface.SyntaxTree);
				var symbol = semanticModel.GetDeclaredSymbol(@interface) as INamedTypeSymbol;

				// does the symbol start with ICommand? Is it generic? Is it in the Mandatum.Commands namespace? If so then heck yeah!
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