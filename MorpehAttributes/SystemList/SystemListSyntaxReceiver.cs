using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MorpehAttributes.Shared.SystemList;

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
            .SelectMany(list => list.Attributes)
            .Where(attributeSyntax => attributeSyntax.Name.ToString() == nameof(SystemListAttribute))
            .ToArray();

        var symbol = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax);
        if (attributes.Length > 0 && symbol != null)
        {
            Test.Add(symbol, attributes);
        }
    }
}