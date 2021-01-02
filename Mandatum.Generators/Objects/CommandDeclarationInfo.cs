using System;
using Microsoft.CodeAnalysis;

namespace Mandatum.Generators
{
	public struct CommandDeclarationInfo
	{
		public string ContainingNamespace { get; set; }
		public string CommandName { get; set; }
		public ITypeSymbol[] Arguments { get; set; }
	}
}