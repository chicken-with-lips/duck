using Duck.Ecs;
using Duck.Scene.Events;
using Duck.Serialization;

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
            World.IsActive = value;
        }
    }

    public string Name => _name;
    public IWorld World => _world;
    public int[] Renderables => _renderables.ToArray();

    #endregion

    #region Members

    private readonly string _name;
    private readonly IWorld _world;
    private readonly List<int> _renderables = new();
    private bool _isActive;
    private bool _shouldFireActivated;

    #endregion

    #region Methods

    internal Scene(string name, IWorld world)
    {
        _name = name;
        _world = world;
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
        if (_shouldFireActivated) {
            _world.CreateOneShot((ref SceneWasMadeActive cmp) => {
                cmp.Scene = this;
            });

            _shouldFireActivated = false;
        }
    }

    #endregion
}
