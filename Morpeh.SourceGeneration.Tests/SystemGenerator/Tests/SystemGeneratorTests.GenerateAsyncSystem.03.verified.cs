﻿//HintName: TestAsyncSystem.g.cs
using Scellecs.Morpeh;

namespace TestFeature.TestAsyncSystem
{
    public partial class TestAsyncSystem : VContainer.Unity.IAsyncStartable, VContainer.Unity.ITickable
    {
        private World _world;

        public void Initialize(Scellecs.Morpeh.World world)
        {
            _world = world;

            // Stashes
            _stash1 = _world.GetStash<Component1>();
            _stash2 = _world.GetStash<Component2>();

            // Filters
            _filter1 = _world.Filter.With<Component1>().With<Component2>().Without<Component3>().Build();
            _filter2 = _world.Filter.With<Component1>().Build();
        }

        public async Cysharp.Threading.Tasks.UniTask CallStartAsync(System.Threading.CancellationToken cancellationToken)
        {
            await StartAsync(cancellationToken);
            _world.Commit();
        }

        public void CallUpdate()
        {
            Tick();
            _world.Commit();
        }

        public void Dispose()
        {
            _filter1 = null;
            _filter2 = null;
        }
    }
}
