//HintName: FeatureRunner.g.cs
namespace Features.FeatureRunner
{
    public partial class FeatureRunner
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

        public void Initialize()
        {
            _firstFeature.Initialize();
            _secondFeature.Initialize();
        }

        public async Cysharp.Threading.Tasks.UniTask StartAsync(System.Threading.CancellationToken cancellation)
        {
            await _firstFeature.StartAsync(cancellation);
            await _secondFeature.StartAsync(cancellation);
        }

        public void Tick()
        {
            _firstFeature.Tick();
            _secondFeature.Tick();
        }

        public void Dispose()
        {
            _firstFeature.Dispose();
            _secondFeature.Dispose();
        }
    }
}
