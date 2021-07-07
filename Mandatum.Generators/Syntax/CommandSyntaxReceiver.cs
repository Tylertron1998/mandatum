using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Mandatum.Generators.Syntax
{
	public class CommandSyntaxReceiver : ISyntaxReceiver
	{
		public List<ClassDeclarationSyntax> Classes { get; }
		public List<InterfaceDeclarationSyntax> Interfaces { get; }

		public CommandSyntaxReceiver()
		{
			Classes = new List<ClassDeclarationSyntax>();
			Interfaces = new List<InterfaceDeclarationSyntax>();
		}
		
		public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
		{
			if (syntaxNode is ClassDeclarationSyntax @class)
			{
				Classes.Add(@class);
			}
			if (syntaxNode is InterfaceDeclarationSyntax @interface)
			{
				Interfaces.Add(@interface);
			}
		}
	}
}