//HintName: FeatureRunnerWithWorld.g.cs
namespace Features.FeatureRunner
{
    public partial class FeatureRunnerWithWorld : System.IDisposable
    {
        private readonly Scellecs.Morpeh.World _world = Scellecs.Morpeh.World.Create();

        public FeatureRunnerWithWorld()
        {
        }

        public void Inject(VContainer.IObjectResolver objectResolver, VContainer.IContainerBuilder builder)
        {
        }

        public void Initialize()
        {
        }

        public void Start()
        {
        }

        public void Update()
        {
        }

        public void Dispose()
        {
        }
    }
}
