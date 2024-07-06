namespace Morpeh.SourceGeneration.Test.SystemGenerator;

[UsesVerify]
public class SystemGeneratorTests
{
    [Fact]
    public Task GenerateSystem()
    {
        const string source =
            """
            public struct Component1 {}
            public struct Component2 {}
            public struct Component3 {}
            
            namespace TestFeature.TestSystem
            {
                public partial class TestSystem : IUpdateSystem
                {
                    [Inject] private ISomeService _someService;
                    
                    private Stash<Component1> _stash1;
                    private Stash<Component2> _stash2;
                    
                    private Filter<(Component1, Component2), Component3> _filter1;
                    private Filter<Component1> _filter2;
                    private Filter<Component1, (Component2, Component3)> _filter3;
            
                    public void Start() { }
            
                    public void Update()
                    {
                        foreach (var entity in _filter1)
                        {
                            // Some code
                        }
                    }
                }
            }
            """;
        
        return TestHelper.Verify<SourceGeneration.SystemGenerator.SystemGenerator>(source, "SystemGenerator/Tests");
    }
    
    [Fact]
    public Task GenerateInitAndUpdateSystem()
    {
        const string source =
            """
            public struct Component1 {}
            public struct Component2 {}
            public struct Component3 {}

            namespace TestFeature.TestInitAndUpdateSystem
            {
                public partial class TestInitAndUpdateSystem : IUpdateSystem, IInitializeSystem
                {
                    [Inject] private ISomeService _someService;
                    
                    private Stash<Component1> _stash1;
                    private Stash<Component2> _stash2;
                    
                    private Filter<(Component1, Component2), Component3> _filter1;
                    private Filter<Component1> _filter2;
                    private Filter<Component1, (Component2, Component3)> _filter3;
            
                    public void Update()
                    {
                        foreach (var entity in _filter1)
                        {
                            // Some code
                        }
                    }
                }
            }
            """;
        
        return TestHelper.Verify<SourceGeneration.SystemGenerator.SystemGenerator>(source, "SystemGenerator/Tests");
    }
}