using Microsoft.CodeAnalysis;

namespace Morpeh.SourceGeneration.SystemGenerator;

internal record struct SystemToGenerate(
    ITypeSymbol TypeSymbol, SystemType SystemType, StashToGenerate[] Stashes, FilterToGenerate[] Filters);

internal record struct StashToGenerate(ISymbol Type, string Name);
internal record struct FilterToGenerate(string Name, ISymbol[] With, ISymbol[] Without);