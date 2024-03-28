//HintName: TestSystem.g.cs
namespace TestFeature.TestSystem
{
	public partial class TestSystem : IAsyncStartable, ITickable
	{
		public void Initialize()
		{
			// Stashes
			_stash1 = _world.GetStash<Component1>();
			_stash2 = _world.GetStash<Component2>();

			// Filters
			_filter1 = _world.Filter.With<Component1>().With<Component2>().Without<Component3>().Build();
			_filter2 = _world.Filter.With<Component1>().Build();
		}

		public void Dispose()
		{
			_filter1 = null;
			_filter2 = null;
		}
	}
}
