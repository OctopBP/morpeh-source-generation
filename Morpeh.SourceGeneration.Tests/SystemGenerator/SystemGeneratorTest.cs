namespace Morpeh.SourceGeneration.Test.SystemGenerator;

[UsesVerify]
public class SystemGeneratorTests
{
    [Fact]
    public Task GenerateClass()
    {
        const string source =
            """
            namespace TestFeature.TestSystem
            {
                public partial class TestSystem : IUpdateSystem
                {
                    [Inject] private Config _config;
                    
                    private Stash<Cooldown> _cooldown;
                    private Stash<GameObjectRef> _gameObjectRef;
                    
                    [With(typeof(Cooldown), typeof(BulletCreate)), Without(typeof(Enemy))]
                    private Filter _filter;
            
                    public async UniTask StartAsync(CancellationToken cancellation) { }
                    // public void Start() { }
            
                    public void Update()
                    {
                        foreach (var entity in _filter)
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