namespace Morpeh.SourceGeneration.Test.FeatureGenerator;

[UsesVerify]
public class FeatureGeneratorTests
{
    [Fact]
    public Task GenerateFeature()
    {
        const string source =
            """
            public interface IInitializeSystem { }
            public interface IAsyncInitializeSystem { }
            public interface IUpdateSystem { }
            
            namespace Features.TestFeature.Systems
            {{
                public class FirstSystem : IAsyncInitializeSystem { }
                public class SecondSystem : IUpdateSystem { }
            }}
            
            namespace Features.TestFeature
            {
                public class TestFeature : IFeature
                {
                    private readonly Features.TestFeature.Systems.FirstSystem _firstSystem;
                    private readonly Features.TestFeature.Systems.SecondSystem _secondSystem;
                }
            }
            """;
        
        return TestHelper.Verify<SourceGeneration.FeatureGenerator.FeatureGenerator>(source, "FeatureGenerator/Tests");
    }
}