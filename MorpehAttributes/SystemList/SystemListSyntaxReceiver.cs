using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MorpehAttributes.SystemList;

internal class SystemListSyntaxReceiver : ISyntaxContextReceiver
{
    public readonly Dictionary<ISymbol, AttributeSyntax[]> Test = new();

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if (context.Node is not ClassDeclarationSyntax { AttributeLists.Count: > 0 } classDeclarationSyntax)
        {
            return;
        }
        
        var attributes = classDeclarationSyntax.AttributeLists
            .SelectMany(l => l.Attributes)
            .Where(e => e.Name.ToString() == "ECSSystem")
            .ToArray();

        var symbol = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax);
        if (attributes.Length > 0 && symbol != null)
        {
            Test.Add(symbol, attributes);
        }
    }
}