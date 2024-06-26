﻿//HintName: TestFeature.g.cs
namespace Features.TestFeature
{
    public partial class TestFeature
    {
        public TestFeature()
        {
            _firstSystem = new Features.TestFeature.Systems.FirstSystem();
            _secondSystem = new Features.TestFeature.Systems.SecondSystem();
        }

        public void Inject(VContainer.IObjectResolver objectResolver)
        {
            objectResolver.Inject(_firstSystem);
            objectResolver.Inject(_secondSystem);
        }

        public void Initialize(Scellecs.Morpeh.World world)
        {
            _firstSystem.Initialize(world);
            _secondSystem.Initialize(world);
        }

        public async Cysharp.Threading.Tasks.UniTask StartAsync(System.Threading.CancellationToken cancellation)
        {
            await _firstSystem.CallStartAsync(cancellation);
        }

        public void Update()
        {
            _secondSystem.CallUpdate();
        }

        public void Dispose()
        {
            _firstSystem.Dispose();
            _secondSystem.Dispose();
        }
    }
}
