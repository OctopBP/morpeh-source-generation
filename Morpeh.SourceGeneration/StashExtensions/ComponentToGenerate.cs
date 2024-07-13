using Microsoft.CodeAnalysis;

namespace Morpeh.SourceGeneration.StashExtensions;

internal record ComponentToGenerate(ITypeSymbol TypeSymbol, FieldToGenerate[] Fields)
{
    public ITypeSymbol TypeSymbol { get; } = TypeSymbol;
    public FieldToGenerate[] Fields { get; } = Fields;
}

internal record struct FieldToGenerate(ISymbol Type, string Name);