using Duck.Ecs;

namespace Duck.Scene;

public class Scene : IScene
{
    #region Properties

    public IWorld World { get; }

    #endregion

    #region Members

    #endregion

    #region Methods

    internal Scene(IWorld world)
    {
        World = world;
    }

    #endregion
}
