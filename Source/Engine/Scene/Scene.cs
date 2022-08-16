using Duck.Ecs;
using Duck.Ecs.Systems;
using Duck.Serialization;

namespace Duck.Scene;

[AutoSerializable]
public partial class Scene : IScene
{
    #region Properties

    public string Name => _name;
    public IWorld World => _world;
    public ISystemComposition SystemComposition => _systemComposition;
    public int[] Renderables => _renderables.ToArray();

    #endregion

    #region Members

    private readonly string _name;
    private readonly IWorld _world;
    private readonly ISystemComposition _systemComposition;
    private readonly List<int> _renderables = new();

    #endregion

    #region Methods

    internal Scene(string name, IWorld world)
    {
        _name = name;
        _world = world;
        _systemComposition = new SystemComposition(world);
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

    #endregion
}
