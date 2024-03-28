using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Morpeh.SourceGeneration.Common;

public static class NameSyntaxExtensions
{
    public static bool AttributeIsEqualByName(this NameSyntax syntax, string attributeName)
    {
        var nameText = syntax.GetNameText();
        return nameText == attributeName || nameText == attributeName.WithAttributePostfix();
    }
}