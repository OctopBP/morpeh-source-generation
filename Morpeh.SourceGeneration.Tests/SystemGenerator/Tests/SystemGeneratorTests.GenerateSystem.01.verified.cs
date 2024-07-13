//HintName: TestFeature.TestSystem.TestSystem.g.cs
using Scellecs.Morpeh;

namespace TestFeature.TestSystem
{
    public partial class TestSystem : System.IDisposable
    {
        private World _world;

        public void Initialize(World world)
        {
            _world = world;

            // Stashes
            _stash1 = _world.GetStash<Component1>();
            _stash2 = _world.GetStash<Component2>();

            // Filters
            _filter1 = new Filter<(Component1, Component2), Component3>(_world.Filter.With<Component1>().With<Component2>().Without<Component3>().Build());
            _filter2 = new Filter<Component1>(_world.Filter.With<Component1>().Build());
            _filter3 = new Filter<Component1, (Component2, Component3)>(_world.Filter.With<Component1>().Without<Component2>().Without<Component3>().Build());
        }

        public void CallUpdate()
        {
            Update();
            _world.Commit();
        }

        public void Dispose()
        {
            _filter1 = null;
            _filter2 = null;
            _filter3 = null;
        }
    }
}
