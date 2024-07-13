//HintName: TestFeature.g.cs
using VContainer;

namespace Features.TestFeature
{
    public partial class TestFeature : System.IDisposable
    {
        public TestFeature()
        {
            _firstSystem = new Features.TestFeature.Systems.FirstSystem();
            _secondSystem = new Features.TestFeature.Systems.SecondSystem();
        }

        public void Inject(VContainer.IObjectResolver objectResolver, VContainer.IContainerBuilder builder)
        {
            objectResolver.Inject(_firstSystem);
            objectResolver.Inject(_secondSystem);

            builder.RegisterInstance(_firstSystem);
        }

        public void Initialize(Scellecs.Morpeh.World world)
        {
            _firstSystem.Initialize(world);
            _secondSystem.Initialize(world);
        }

        public void Start()
        {
            _firstSystem.CallStart();
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
