using Arch.Core;
using Duck.Platform;

namespace Duck.Scene;

public class Scene : IScene
{
    #region Properties

    public bool IsActive
    {
        get => _isActive;
        set => _isActive = value;
    }

    public string Name => _name;
    public World World => _world;
    public SceneSystemRoot<FrameTimer> SystemRoot => _systemRoot;

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
        _systemRoot = new(world);
    }

    public void PreTick(in FrameTimer timer)
    {
        if (!IsActive) {
            return;
        }

        _systemRoot.EarlySimulationGroup.BeforeUpdate(timer);
        _systemRoot.EarlySimulationGroup.Update(timer);
        _systemRoot.EarlySimulationGroup.AfterUpdate(timer);

        _systemRoot.SimulationGroup.BeforeUpdate(timer);
    }

    public void Tick(in FrameTimer timer)
    {
        if (!IsActive) {
            return;
        }

        _systemRoot.SimulationGroup.Update(timer);
    }

    public void FixedTick(in FrameTimer timer)
    {
        if (!IsActive) {
            return;
        }

        _systemRoot.FixedSimulationGroup.Update(timer);
    }

    public void PostTick(in FrameTimer timer)
    {
        if (!IsActive) {
            return;
        }

        _systemRoot.SimulationGroup.AfterUpdate(timer);

        _systemRoot.LateSimulationGroup.BeforeUpdate(timer);
        _systemRoot.LateSimulationGroup.Update(timer);
        _systemRoot.LateSimulationGroup.AfterUpdate(timer);
    }

    public void Present(in FrameTimer timer)
    {
        if (!IsActive) {
            return;
        }

        // _systemRoot.PresentationGroup.BeforeUpdate(timer);
        // _systemRoot.PresentationGroup.Update(timer);
        // _systemRoot.PresentationGroup.AfterUpdate(timer);
    }
}
