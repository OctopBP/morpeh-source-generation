using System;
using Microsoft.CodeAnalysis;

namespace Morpeh.SourceGeneration.SystemGenerator;

[Flags]
internal enum SystemType
{
    None = 0,
    Initialize = 1,
    Update = 2,
}

internal record struct SystemToGenerate(
    ITypeSymbol TypeSymbol, SystemType SystemType, StashToGenerate[] Stashes, FilterToGenerate[] Filters);

internal record struct StashToGenerate(ISymbol Type, string Name);
internal record struct FilterToGenerate(string Name, ISymbol[] With, ISymbol[] Without);