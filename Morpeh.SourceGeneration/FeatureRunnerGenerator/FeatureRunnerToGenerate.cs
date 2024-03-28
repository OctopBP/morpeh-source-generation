using Microsoft.CodeAnalysis;

namespace Morpeh.SourceGeneration.FeatureRunnerGenerator;

internal record struct FeatureRunnerToGenerate(ITypeSymbol TypeSymbol, FeaturesToGenerate[] Systems);
internal record struct FeaturesToGenerate(ISymbol Type, string Name);