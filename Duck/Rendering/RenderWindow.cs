using System.Numerics;
using Filament;
using Filament.CameraUtilities;

namespace Duck.Rendering
{
    public class RenderWindow
    {
        #region Properties

        internal Filament.View View { get; }

        #endregion

        #region Members

        private readonly SwapChain _swapChain;
        private readonly Renderer _renderer;

        private readonly Camera _defaultCamera;
        private readonly Scene _defaultScene;

        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;
        private int renderable;

        #endregion

        #region Methods

        internal RenderWindow(Engine engine, NativeWindow nativeWindow)
        {
            _swapChain = engine.CreateSwapChain(nativeWindow.Ptr);

            _defaultCamera = engine.CreateCamera(
                EntityManager.Create()
            );
            _defaultScene = engine.CreateScene();

            View = engine.CreateView();
            View.Scene = _defaultScene;
            View.Camera = _defaultCamera;

            _renderer = engine.CreateRenderer();
            _renderer.SetClearOptions(new Vector4(0, 0, 0, 0), true, true);

            Resize(nativeWindow.Width, nativeWindow.Height);
        }

        public void Resize(int newWidth, int newHeight)
        {
            View.Viewport = new Viewport(0, 0, newWidth, newHeight);
        }

        public void Render()
        {
            if (_renderer.BeginFrame(_swapChain)) {
                _renderer.Render(View);
                _renderer.EndFrame();
            }
        }

        #endregion
    }
}
