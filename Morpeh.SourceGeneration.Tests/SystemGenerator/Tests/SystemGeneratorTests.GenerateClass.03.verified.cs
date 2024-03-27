//HintName: TestSystem.g.cs
public partial class TestSystem : IAsyncStartable, ITickable
{
    public void Initialize()
    {
        // Stashes
        _cooldown = _world.GetStash<Cooldown>();
        _gameObjectRef = _world.GameObjectRef<Cooldown>();
        
        // Filters
        _filter = _world.Filter.With<Cooldown>().With<BulletCreate>().Without<Enemy>().Build();
    }
    
    public void Dispose()
    {
        _filter = null;
    }
}