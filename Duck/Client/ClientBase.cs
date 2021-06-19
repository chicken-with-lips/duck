using Duck.Contracts;
using Duck.Contracts.Input;
using Duck.Contracts.SceneManagement;
using Duck.Ecs;
using Duck.Ecs.Systems;
using Duck.Game;

namespace Duck.Client
{
    public abstract class ClientBase : IClient
    {
        #region Members

        private IScene? _scene;
        private ISystemComposition? _systemComposition;

        #endregion

        #region IClient

        public void Initialize(IClientInitializationContext context)
        {
            var app = context.Application;
            var sceneSubsystem = app.GetSubsystem<ISceneSubsystem>();

            _scene = sceneSubsystem.Create();
            _systemComposition = app.CreateDefaultSystemComposition(_scene);

            InitializeInput(app.GetSubsystem<IInputSubsystem>());
            InitializeSystems(_systemComposition, context);

            _systemComposition.Init();
        }

        protected abstract void InitializeInput(IInputSubsystem input);

        protected abstract void InitializeSystems(ISystemComposition composition, IClientInitializationContext context);

        public void Tick()
        {
            _systemComposition?.Tick();
        }

        #endregion
    }
}
