using Microsoft.CodeAnalysis;
using Morpeh.SourceGeneration.SystemGenerator;

namespace Morpeh.SourceGeneration.FeatureGenerator;

internal record struct FeatureToGenerate(ITypeSymbol TypeSymbol, SystemToGenerate[] Systems);
internal record struct SystemToGenerate(ISymbol Type, SystemType SystemType, string Name);