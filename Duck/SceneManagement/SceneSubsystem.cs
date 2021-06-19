using System.Collections.Generic;
using System.Numerics;
using Duck.Contracts.SceneManagement;
using Duck.Ecs;
using Duck.Rendering;
using Filament;

namespace Duck.SceneManagement
{
    public class SceneSubsystem : ISceneSubsystem
    {
        #region Members

        private readonly IWorldSubsystem _worldSubsystem;
        private readonly RenderingSubsystem _renderingSubsystem;

        private readonly List<IScene> _loadedScenes = new();

        #endregion

        #region Methods

        public SceneSubsystem(IWorldSubsystem worldSubsystem, RenderingSubsystem renderingSubsystem)
        {
            _worldSubsystem = worldSubsystem;
            _renderingSubsystem = renderingSubsystem;
        }

        public IScene Create()
        {
            var renderingScene = _renderingSubsystem.CreateRenderingScene();
            _renderingSubsystem.PrimaryView.Scene = renderingScene;

            var scene = new Scene(
                _worldSubsystem.Create(),
                _renderingSubsystem.Engine,
                _renderingSubsystem.PrimaryView,
                renderingScene
            );

            _loadedScenes.Add(scene);

            return scene;
        }

        #endregion
    }
}
