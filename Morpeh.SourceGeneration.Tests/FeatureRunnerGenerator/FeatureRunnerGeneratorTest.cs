namespace Morpeh.SourceGeneration.Test.FeatureRunnerGenerator;

[UsesVerify]
public class FeatureRunnerGeneratorTest
{
    [Fact]
    public Task GenerateFeatureRunner()
    {
        const string source =
            """
            namespace Features
            {{
                public class FirstFeature {}
                public class SecondFeature {}
            }}
            
            namespace Features.FeatureRunner
            {
                public class FeatureRunner : IFeatureRunner
                {
                    private readonly Features.FirstFeature _firstFeature;
                    private readonly Features.SecondFeature _secondFeature;
                }
            }
            """;
        
        return TestHelper.Verify<SourceGeneration.FeatureRunnerGenerator.FeatureRunnerGenerator>(
            source, "FeatureRunnerGenerator/Tests"
        );
    }
}