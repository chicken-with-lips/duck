using Arch.System;

namespace Duck.Renderer;

public class SystemRoot
{
    public Group<float> InitializationGroup { get; }
    public Group<float> SimulationGroup { get; }
    public Group<float> LateSimulationGroup { get; }
    public PresentationGroup<float> PresentationGroup { get; }
    public Group<float> ExitFrameGroup { get; }

    private readonly Group<float> _root = new();

    public SystemRoot()
    {
        _root
            .Add(InitializationGroup = new Group<float>())
            .Add(SimulationGroup = new Group<float>())
            .Add(LateSimulationGroup = new Group<float>())
            .Add(PresentationGroup = new PresentationGroup<float>())
            .Add(ExitFrameGroup = new Group<float>());
    }

    public void Initialize()
    {
        _root.Initialize();
    }

    public void BeforeUpdate(float deltaTime)
    {
        _root.BeforeUpdate(deltaTime);
    }

    public void Update(float deltaTime)
    {
        _root.Update(deltaTime);
    }

    public void AfterUpdate(float deltaTime)
    {
        _root.AfterUpdate(deltaTime);
    }
}

public class PresentationGroup<T> : ISystem<T>
{
    public CommandBuffer? CommandBuffer { get; set; }
    public View? View { get; set; }

    private readonly List<ISystem<T>> _systems = new();

    public PresentationGroup<T> Add(params ISystem<T>[] systems)
    {
        _systems.AddRange(systems);

        return this;
    }

    public void Initialize()
    {
        foreach (var system in _systems) {
            system.Initialize();
        }
    }

    public void BeforeUpdate(in T t)
    {
        foreach (var system in _systems) {
            if (system is IPresentationSystem presentationSystem) {
                presentationSystem.CommandBuffer = CommandBuffer;
                presentationSystem.View = View;
            }

            system.BeforeUpdate(t);
        }
    }

    public void Update(in T t)
    {
        foreach (var system in _systems) {
            system.Update(t);
        }
    }

    public void AfterUpdate(in T t)
    {
        foreach (var system in _systems) {
            system.AfterUpdate(t);
        }
    }

    public void Dispose()
    {
        foreach (ISystem<T> system in _systems) {
            system.Dispose();
        }
    }
}

public interface IPresentationSystem
{
    public CommandBuffer? CommandBuffer { get; set; }
    public View? View { get; set; }
}
