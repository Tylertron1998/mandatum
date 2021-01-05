using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace Mandatum.Generators
{
	public class ResolverSyntaxFinder
	{
		public IEnumerable<INamedTypeSymbol> FindResolverSymbols(Compilation compilation)
		{
			return new[]
			{
				compilation.GetTypeByMetadataName("Mandatum.Resolvers.Int32Resolver"),
				compilation.GetTypeByMetadataName("Mandatum.Resolvers.HttpStatusCodeResolver")
			};
		}
	}
}