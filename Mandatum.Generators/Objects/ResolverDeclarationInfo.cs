using Microsoft.CodeAnalysis;

namespace Mandatum.Generators.Objects
{
	public class ResolverDeclarationInfo
	{
		public string Name { get; set; }
		public string ContainingNamespace { get; set; }
		public ITypeSymbol ConversionType { get; set; }
		public bool IsAsync { get; set; }
	}
}