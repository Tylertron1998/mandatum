using System.Collections.Generic;
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
	}
}