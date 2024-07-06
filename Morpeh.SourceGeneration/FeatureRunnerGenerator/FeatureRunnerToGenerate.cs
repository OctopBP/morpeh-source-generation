using Microsoft.CodeAnalysis;

namespace Morpeh.SourceGeneration.FeatureRunnerGenerator;

internal record struct FeatureRunnerToGenerate(ITypeSymbol TypeSymbol, FeaturesToGenerate[] Systems, bool WithWorld);
internal record struct FeaturesToGenerate(ISymbol Type, string Name);