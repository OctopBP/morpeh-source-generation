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
            public interface IUpdateSystem { }
            public interface ISomeInterface { }
            
            [System.AttributeUsage(System.AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
            public class RegisterAttribute : System.Attribute { }
            
            namespace Features.TestFeature.Systems
            {{
                public class FirstSystem : IInitializeSystem { }
                public class SecondSystem : IUpdateSystem { }
                public class NotASystem { }
            }}
            
            namespace Features.TestFeature
            {
                public class TestFeature : IFeature
                {
                    [Register] private readonly Features.TestFeature.Systems.FirstSystem _firstSystem;
                    private readonly Features.TestFeature.Systems.SecondSystem _secondSystem;
                    
                    private readonly Features.TestFeature.Systems.NotASystem _notASystem;
                    private readonly ISomeInterface _someInterface;
                }
            }
            """;
        
        return TestHelper.Verify<SourceGeneration.FeatureGenerator.FeatureGenerator>(source, "FeatureGenerator/Tests");
    }
}