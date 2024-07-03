//HintName: FeatureRunner.g.cs
namespace Features.FeatureRunner
{
    public partial class FeatureRunner : System.IDisposable
    {
        public FeatureRunner()
        {
            _firstFeature = new Features.FirstFeature();
            _secondFeature = new Features.SecondFeature();
        }

        public void Inject(VContainer.IObjectResolver objectResolver)
        {
            _firstFeature.Inject(objectResolver);
            _secondFeature.Inject(objectResolver);
        }

        public void Initialize(Scellecs.Morpeh.World world)
        {
            _firstFeature.Initialize(world);
            _secondFeature.Initialize(world);
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
