using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MorpehAttributes.SystemList;

internal class SystemListSyntaxReceiver : ISyntaxContextReceiver
{
    public SystemInfo[] SystemInfos { get; private set; }

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if (context.Node is not ClassDeclarationSyntax { AttributeLists.Count: > 0 } classDeclarationSyntax)
        {
            return;
        }

        SystemInfos = classDeclarationSyntax.AttributeLists
            .SelectMany(l => l.Attributes)
            .Where(e => e.Name.NormalizeWhitespace().ToFullString() == nameof(SystemListAttribute))
            .Select(attributeSyntax =>
            {
                var expressionSyntax = attributeSyntax.ArgumentList?.Arguments[0].Expression;
                var argumentValue =
                    (SystemInfo[]) context.SemanticModel.GetOperation(expressionSyntax)?.ConstantValue.Value!;

                return argumentValue;
            })
            .FirstOrDefault();
    }
}