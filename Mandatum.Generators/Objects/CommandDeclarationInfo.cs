using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Mandatum.Generators.Objects
{
	public struct CommandDeclarationInfo
	{
		public string ContainingNamespace { get; set; }
		public string Name { get; set; }
		public IEnumerable<ITypeSymbol> Arguments { get; set; }
		public string Delimiter { get; set; }
	}
}