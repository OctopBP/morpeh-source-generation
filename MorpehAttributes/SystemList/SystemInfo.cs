using Microsoft.CodeAnalysis.CSharp.Syntax;
using MorpehAttributes.Common;

namespace MorpehAttributes.SystemList;

class SystemInfo
{
    public readonly TypeSyntax TypeSyntax;
    public readonly string Name;
    public readonly string UpdatePhase;
    public readonly string Type;
    public readonly string VarName;
    
    public SystemInfo(TypeSyntax typeSyntax, string updatePhase, string type)
    {
        TypeSyntax = typeSyntax;
        Name = typeSyntax.GetText().ToString();
        UpdatePhase = updatePhase;
        Type = type;
        VarName = Name.LowerFirstLatter().WithUnderscore();
    }
}