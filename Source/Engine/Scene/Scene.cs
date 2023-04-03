using Arch.Core;
using Arch.System;
using Duck.Scene.Events;
using Duck.Serialization;
using Duck.ServiceBus;

namespace Duck.Scene;

[AutoSerializable]
public partial class Scene : IScene
{
    #region Properties

    public bool IsActive {
        get => _isActive;
        set {
            if (value && value != _isActive) {
                _shouldFireActivated = true;
            } else {
                _shouldFireActivated = false;
            }

            _isActive = value;
        }
    }

    public string Name => _name;
    public World World => _world;
    public Group<float> SystemRoot => _systemRoot;

    #endregion

    #region Members

    private readonly string _name;
    private readonly World _world;
    private readonly IEventBus _eventBus;
    private readonly Group<float> _systemRoot;

    private bool _isActive;
    private bool _shouldFireActivated;

    #endregion

    #region Methods

    internal Scene(string name, World world, IEventBus eventBus)
    {
        _name = name;
        _isActive = false;
        _world = world;
        _eventBus = eventBus;
        _systemRoot = new Group<float>();
    }

    public void PreTick(in float deltaTime)
    {
        if (IsActive) {
            _systemRoot.BeforeUpdate(deltaTime);
        }
    }

    public void Tick(in float deltaTime)
    {
        if (_shouldFireActivated) {
            _eventBus.Emit(new SceneWasMadeActive() {
                Scene = this
            });

            _shouldFireActivated = false;
        }

        _systemRoot.Update(deltaTime);
    }

    public void PostTick(in float deltaTime)
    {
        if (IsActive) {
            _systemRoot.AfterUpdate(deltaTime);
        }
    }

    #endregion
}
