using System;
using Duck.Contracts;
using Duck.Contracts.Logging;
using Duck.Contracts.Renderering;
using Filament;

namespace Duck.Rendering
{
    public class RenderingSubsystem : IRenderingSubsystem, IApplicationInitializableSubsystem, IApplicationTickableSubsystem, IApplicationPostTickableSubsystem
    {
        #region Properties

        internal Filament.View PrimaryView => _renderingWindow?.View;
        internal Engine Engine => _engine;

        internal NativeWindow NativeWindow => _nativeWindow;

        #endregion

        #region Members

        private bool _isInitialized;

        private ILogger? _logger;
        private NativeWindow? _nativeWindow;
        private RenderWindow? _renderingWindow;
        private Engine? _engine;

        private readonly IApplication _app;
        private readonly ILogSubsystem _logSubsystem;

        #endregion

        #region Methods

        internal RenderingSubsystem(IApplication app, ILogSubsystem logSubsystem)
        {
            _app = app;
            _logSubsystem = logSubsystem;
        }

        private void CreateNativeWindow()
        {
            _logger?.LogInformation("Creating window.");
            _nativeWindow = new NativeWindow(NativeWindow.Configuration.Default);
        }

        private void InitializeFilament()
        {
            _engine = Engine.Create();
            _renderingWindow = new RenderWindow(_engine, _nativeWindow);
        }

        #endregion

        #region IRenderingSubsystem

        public bool Init()
        {
            if (_isInitialized) {
                throw new Exception("RenderingSubsystem has already been initialized");
            }

            _logger = _logSubsystem.CreateLogger("Rendering");
            _logger.LogInformation("Initializing rendering subsystem.");

            CreateNativeWindow();
            InitializeFilament();

            _isInitialized = true;

            return true;
        }

        public void Tick()
        {
            _nativeWindow?.PumpEvents();

            ProcessWindowEvents();

            if (_nativeWindow.CloseRequested) {
                _app.Shutdown();
            } else {
                _renderingWindow?.Render();
            }
        }

        public void PostTick()
        {
            _nativeWindow?.ClearEvents();
        }

        internal Scene CreateRenderingScene()
        {
            return _engine.CreateScene();
        }

        private void ProcessWindowEvents()
        {
            foreach (var windowEvent in _nativeWindow.Events) {
                if (windowEvent is NativeWindow.ResizeEvent resizeEvent) {
                    _renderingWindow?.Resize(resizeEvent.NewWidth, resizeEvent.NewHeight);
                }
            }
        }

        #endregion
    }
}
