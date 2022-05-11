using Duck.Ecs;
using Duck.Serialization;

namespace Duck.Scene;

[AutoSerializable]
public partial class Scene : IScene
{
    #region Properties

    public IWorld World => _world;
    public int[] Renderables => _renderables.ToArray();

    #endregion

    #region Members

    private readonly IWorld _world;
    private readonly List<int> _renderables = new();

    #endregion

    #region Methods

    internal Scene(IWorld world)
    {
        _world = world;
    }

    public IScene AddRenderable(int entityId)
    {
        if (!_renderables.Contains(entityId)) {
            _renderables.Add(entityId);
        }

        return this;
    }

    #endregion
}
