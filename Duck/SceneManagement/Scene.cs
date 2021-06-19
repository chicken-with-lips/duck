using System.Numerics;
using Duck.Contracts.SceneManagement;
using Duck.Ecs;
using Filament;
using RenderingScene = Filament.Scene;
using RenderingView = Filament.View;

namespace Duck.SceneManagement
{
    public class Scene : IScene
    {
        #region Properties

        public IWorld World { get; }
        internal RenderingScene RenderingScene { get; }
        internal RenderingView RenderingView { get; }

        #endregion

        #region Members

        #endregion

        #region Methods

        internal Scene(IWorld world, Engine renderingEngine, RenderingView renderingView, RenderingScene renderingScene)
        {
            World = world;

            RenderingView = renderingView;

            RenderingScene = renderingScene;
            RenderingScene.Skybox = SkyboxBuilder.Create()
                .WithColor(new Color(1, 1, 1, 1.0f))
                .Build(renderingEngine);
            ;

            int light = EntityManager.Create();

            LightBuilder.Create(LightType.Directional)
                .WithColor(Color.FromCorrelatedColorTemperature(15500f))
                // Intensity of the sun in lux on a clear day
                .WithIntensity(1100000)
                .WithDirection(new Vector3(0.0f, -0.5f, -1.0f))
                .WithCastShadows(true)
                .Build(renderingEngine, light);

            RenderingScene.AddEntity(light);
        }

        #endregion
    }
}
