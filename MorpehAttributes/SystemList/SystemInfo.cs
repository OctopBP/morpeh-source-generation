using Microsoft.CodeAnalysis.CSharp.Syntax;
using MorpehAttributes.Common;
using MorpehAttributes.Shared.SystemList;

namespace MorpehAttributes.SystemList;

class SystemInfo
{
    public readonly TypeSyntax TypeSyntax;
    public readonly SystemListAttribute SystemListAttribute;
    public readonly string VarName;
    
    public SystemInfo(TypeSyntax typeSyntax, SystemListAttribute systemListAttribute)
    {
        TypeSyntax = typeSyntax;
        SystemListAttribute = systemListAttribute;
        VarName = systemListAttribute.SystemType.Name.LowerFirstLatter().WithUnderscore();
    }
}