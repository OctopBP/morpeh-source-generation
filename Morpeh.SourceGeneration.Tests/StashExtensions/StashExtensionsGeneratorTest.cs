namespace Morpeh.SourceGeneration.Test.StashExtensions;

[UsesVerify]
public class StashExtensionsGeneratorTest
{
    [Fact]
    public Task GenerateStashExtensions()
    {
        const string source =
            """
            namespace Features.Components
            {
                public struct TestComponent1 : Scellecs.Morpeh.IComponent
                {
                }
                
                public struct TestComponent2 : Scellecs.Morpeh.IComponent
                {
                    public int Value;
                }
                
                public struct TestComponent3 : Scellecs.Morpeh.IComponent
                {
                    public int Value;
                    public Other.TestClass TestClass;
                }
            }
            
            namespace Other
            {
                public class TestClass {}
            }
            """;
        
        return TestHelper.Verify<SourceGeneration.StashExtensions.StashExtensionsGenerator>(
            source, "StashExtensions/Tests"
        );
    }
}