using Arch.Core;
using Duck.Platform;

namespace Duck.Scene;

public class Scene : IScene
{
    #region Properties

    public bool IsActive {
        get => _isActive;
        set => _isActive = value;
    }

    public string Name {
        get => _name;
    }

    public World World {
        get => _world;
    }

    public SceneSystemRoot<FrameTimer> SystemRoot {
        get => _systemRoot;
    }

    #endregion

    #region Members

    private readonly string _name;
    private readonly World _world;
    private readonly SceneSystemRoot<FrameTimer> _systemRoot;

    private bool _isActive;

    #endregion

    internal Scene(string name, World world, bool isActive = false)
    {
        _name = name;
        _isActive = isActive;
        _world = world;
        _systemRoot = new SceneSystemRoot<FrameTimer>(world);
    }

    public void PreTick(FrameTimer timer)
    {
        if (!IsActive) {
            return;
        }

        _systemRoot.EarlySimulationGroup.BeforeUpdate(timer);
        _systemRoot.EarlySimulationGroup.Update(timer);
        _systemRoot.EarlySimulationGroup.AfterUpdate(timer);

        _systemRoot.SimulationGroup.BeforeUpdate(timer);
    }

    public void Tick(FrameTimer timer)
    {
        if (!IsActive) {
            return;
        }

        _systemRoot.SimulationGroup.Update(timer);
    }

    public void FixedTick(FrameTimer timer)
    {
        if (!IsActive) {
            return;
        }

        _systemRoot.FixedSimulationGroup.Update(timer);
    }

    public void PostTick(FrameTimer timer)
    {
        if (!IsActive) {
            return;
        }

        _systemRoot.SimulationGroup.AfterUpdate(timer);

        _systemRoot.LateSimulationGroup.BeforeUpdate(timer);
        _systemRoot.LateSimulationGroup.Update(timer);
        _systemRoot.LateSimulationGroup.AfterUpdate(timer);
    }

    public void Present(FrameTimer timer)
    {
        if (!IsActive) {
            return;
        }

        // _systemRoot.PresentationGroup.BeforeUpdate(timer);
        // _systemRoot.PresentationGroup.Update(timer);
        // _systemRoot.PresentationGroup.AfterUpdate(timer);
    }
}
