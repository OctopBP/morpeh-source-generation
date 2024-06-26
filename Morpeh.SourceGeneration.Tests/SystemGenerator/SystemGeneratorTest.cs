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
                    
                    [With(typeof(Component1), typeof(Component2)), Without(typeof(Component3))]
                    private Filter _filter1;
                    
                    [With(typeof(Component1))]
                    private Filter _filter2;
            
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
    public Task GenerateAsyncSystem()
    {
        const string source =
            """
            public struct Component1 {}
            public struct Component2 {}
            public struct Component3 {}

            namespace TestFeature.TestAsyncSystem
            {
                public partial class TestAsyncSystem : IUpdateSystem, IAsyncInitializeSystem
                {
                    [Inject] private ISomeService _someService;
                    
                    private Stash<Component1> _stash1;
                    private Stash<Component2> _stash2;
                    
                    [With(typeof(Component1), typeof(Component2)), Without(typeof(Component3))]
                    private Filter _filter1;
                    
                    [With(typeof(Component1))]
                    private Filter _filter2;
            
                    public async UniTask StartAsync(CancellationToken cancellation) { }
            
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