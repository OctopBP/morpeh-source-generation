//HintName: FeatureRunnerWithWorld.g.cs
namespace Features.FeatureRunner
{
    public partial class FeatureRunnerWithWorld : System.IDisposable
    {
        private readonly Scellecs.Morpeh.World _world = Scellecs.Morpeh.World.Create();

        public FeatureRunnerWithWorld()
        {
            _firstFeature = new Features.FirstFeature();
            _secondFeature = new Features.SecondFeature();
        }

        public void Inject(VContainer.IObjectResolver objectResolver)
        {
            _firstFeature.Inject(objectResolver);
            _secondFeature.Inject(objectResolver);
        }

        public void Initialize()
        {
            _firstFeature.Initialize(_world);
            _secondFeature.Initialize(_world);
        }

        public void Start()
        {
            _firstFeature.Start();
            _secondFeature.Start();
        }

        public void Update()
        {
            _firstFeature.Update();
            _secondFeature.Update();
        }

        public void Dispose()
        {
            _firstFeature.Dispose();
            _secondFeature.Dispose();
        }
    }
}
