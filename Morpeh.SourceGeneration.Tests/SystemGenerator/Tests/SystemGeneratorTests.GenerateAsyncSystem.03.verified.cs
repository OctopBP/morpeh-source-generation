//HintName: TestAsyncSystem.g.cs
using Scellecs.Morpeh;

namespace TestFeature.TestAsyncSystem
{
    public partial class TestAsyncSystem : VContainer.Unity.IAsyncStartable, VContainer.Unity.ITickable
    {
        public void Initialize(Scellecs.Morpeh.World world)
        {
            // Stashes
            _stash1 = world.GetStash<Component1>();
            _stash2 = world.GetStash<Component2>();

            // Filters
            _filter1 = world.Filter.With<Component1>().With<Component2>().Without<Component3>().Build();
            _filter2 = world.Filter.With<Component1>().Build();
        }

        public void Dispose()
        {
            _filter1 = null;
            _filter2 = null;
        }
    }
}
