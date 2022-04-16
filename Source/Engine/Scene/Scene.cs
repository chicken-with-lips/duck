using Duck.Ecs;
using Duck.Serialization;

namespace Duck.Scene;

[AutoSerializable]
public partial class Scene : IScene
{
    #region Properties

    public IWorld World => _world;
    
    #endregion
    
    #region Members

    private readonly IWorld _world;

    #endregion

    #region Methods

    internal Scene(IWorld world)
    {
        _world = world;
    }

    #endregion
}
