using System;
using System.Collections.Generic;
using Duck.Contracts;
using Duck.Contracts.Logging;
using Duck.Contracts.SceneManagement;
using Duck.Contracts.ServiceBus;
using Duck.Ecs;
using Duck.Ecs.Systems;
using Duck.Exceptions;
using Duck.FilamentBridge.Systems;
using Duck.Input;
using Duck.Logging;
using Duck.Physics;
using Duck.Physics.Systems;
using Duck.Renderering.Systems;
using Duck.Rendering;
using Duck.Rendering.Systems;
using Duck.SceneManagement;
using Duck.ServiceBus;
using Duck.Timing;
using static GLFWDotNet.GLFW;

namespace Duck
{
    public abstract class BaseApplication : IApplication
    {
        #region Members

        private State _state = State.Uninitialized;
        private ILogger _systemLogger;
        private readonly FrameTimer _frameTimer;

        private readonly List<IApplicationSubsystem> _subsystems = new();

        private bool _isEditor;

        #endregion

        public BaseApplication(bool isEditor)
        {
            glfwInit();

            _frameTimer = new();
            _isEditor = isEditor;

            RegisterSubsystems();
        }

        public bool Init()
        {
            if (_state != State.Uninitialized) {
                throw new Exception("Application already initialized");
            }

            ChangeState(State.Initializing);

            Time.InternalFrameTimer = _frameTimer;

            if (!InitializeSubsystems()) {
                return false;
            }

            ChangeState(State.Initialized);

            return true;
        }

        public T GetSubsystem<T>() where T : IApplicationSubsystem
        {
            foreach (var subsystem in _subsystems) {
                if (subsystem is T applicationSubsystem) {
                    return applicationSubsystem;
                }
            }

            throw new ApplicationSubsystemNotFoundException();
        }

        private bool InitializeSubsystems()
        {
            foreach (var subsystem in _subsystems) {
                if (subsystem is IApplicationInitializableSubsystem initializable) {
                    if (!initializable.Init()) {
                        return false;
                    }
                }
            }

            _systemLogger = GetSubsystem<ILogSubsystem>().CreateLogger("System");

            return true;
        }

        private void PreTickSubsystems()
        {
            foreach (var subsystem in _subsystems) {
                if (subsystem is IApplicationPreTickableSubsystem tickable) {
                    tickable.PreTick();
                }
            }
        }

        private void TickSubsystems()
        {
            GetSubsystem<IEventBus>().Emit();

            foreach (var subsystem in _subsystems) {
                if (subsystem is IApplicationTickableSubsystem tickable) {
                    tickable.Tick();
                }
            }
        }

        private void PostTickSubsystems()
        {
            foreach (var subsystem in _subsystems) {
                if (subsystem is IApplicationPostTickableSubsystem tickable) {
                    tickable.PostTick();
                }
            }
        }

        private void RegisterSubsystems()
        {
            _subsystems.Add(new LogSubsystem());
            _subsystems.Add(new EventBus());
            _subsystems.Add(new RenderingSubsystem(this, GetSubsystem<ILogSubsystem>()));
            _subsystems.Add(new WorldSubsystem(GetSubsystem<ILogSubsystem>(), GetSubsystem<IEventBus>()));
            _subsystems.Add(new SceneSubsystem(GetSubsystem<IWorldSubsystem>(), GetSubsystem<RenderingSubsystem>()));

            if (_isEditor) {
            } else {
                _subsystems.Add(new InputSubsystem(GetSubsystem<ILogSubsystem>(), GetSubsystem<RenderingSubsystem>()));
                _subsystems.Add(new PhysicsSubsystem(GetSubsystem<ILogSubsystem>(), GetSubsystem<IEventBus>()));
            }
        }

        public void Run()
        {
            if (_state != State.Initialized) {
                throw new Exception("Application has not been initialized");
            }

            ChangeState(State.Running);

            while (_state == State.Running) {
                _frameTimer.Update();

                PreTickSubsystems();
                TickSubsystems();
                PostTickSubsystems();
            }
        }

        public void Shutdown()
        {
            if (_state == State.TearingDown) {
                return;
            }

            _systemLogger?.LogInformation("Shutdown requested");

            ChangeState(State.TearingDown);
        }

        public void AddSubsystem(IApplicationSubsystem subsystem)
        {
            _subsystems.Add(subsystem);
        }

        public ISystemComposition CreateDefaultSystemComposition(IScene scene)
        {
            var c = new SystemComposition(scene.World);
            c
                .Add(new GenerateFilamentIdentitySystem())
                .Add(new CameraLifecycleSystem())
                .Add(new BoxPrimitiveLifecycleSystem())
                .Add(new MeshLifecycleSystem())
                .Add(new PhysicsBoxShapeLifecycleSystem())
                .Add(new SyncPhysicsTransformsSystem())
                .Add(new SyncRenderTransformsSystem());

            return c;
        }

        private void ChangeState(State newState)
        {
            _state = newState;
        }

        public enum State
        {
            Uninitialized,
            Initializing,
            Initialized,
            Running,
            TearingDown,
        }
    }
}
