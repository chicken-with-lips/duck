using Duck.Ecs;
using Duck.Ecs.Systems;
using Duck.Scene.Events;
using Duck.Scene.Scripting;
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
                _eventBus.Enqueue(new SceneWasMadeActive(this));
                _shouldFireActivated = true;
            } else {
                _shouldFireActivated = false;
            }

            _isActive = value;
        }
    }

    public ISceneScript? Script { get; set; }

    public string Name => _name;
    public IWorld World => _world;
    public ISystemComposition SystemComposition => _systemComposition;
    public int[] Renderables => _renderables.ToArray();

    #endregion

    #region Members

    private readonly string _name;
    private readonly IWorld _world;
    private readonly IEventBus _eventBus;
    private readonly ISystemComposition _systemComposition;
    private readonly List<int> _renderables = new();
    private bool _isActive;
    private bool _shouldFireActivated;

    #endregion

    #region Methods

    internal Scene(string name, IWorld world, IEventBus eventBus)
    {
        _name = name;
        _world = world;
        _eventBus = eventBus;
        _systemComposition = new SystemComposition(world);
        _isActive = false;
    }

    public IScene AddRenderable(int entityId)
    {
        if (!_renderables.Contains(entityId)) {
            _renderables.Add(entityId);
        }

        return this;
    }

    public IScene RemoveRenderable(int entityId)
    {
        _renderables.Remove(entityId);

        return this;
    }

    public void Tick()
    {
        if ( _shouldFireActivated && Script is ISceneMadeActive madeActive) {
            madeActive.OnActivated();

            _shouldFireActivated = false;
        }

        if (_isActive) {
            SystemComposition.Tick();

            if (Script is ISceneTick tick) {
                tick.OnTick();
            }
        }
    }

    #endregion
}
