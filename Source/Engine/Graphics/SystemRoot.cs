using Arch.Core;
using Arch.System;

namespace Duck.Graphics;

public class SystemRoot : IDisposable
{
    public Group<float> InitializationGroup { get; }
    public Group<float> SimulationGroup { get; }
    public Group<float> EarlySimulationGroup { get; }
    public Group<float> LateSimulationGroup { get; }
    public PresentationGroup<float> PresentationGroup { get; }
    public Group<float> ExitFrameGroup { get; }

    private readonly Group<float> _root;

    public SystemRoot(World world)
    {
        _root = new(world);

        _root
            .Add(InitializationGroup = new Group<float>(world))
            .Add(EarlySimulationGroup = new Group<float>(world))
            .Add(SimulationGroup = new Group<float>(world))
            .Add(LateSimulationGroup = new Group<float>(world))
            .Add(PresentationGroup = new PresentationGroup<float>(world))
            .Add(ExitFrameGroup = new Group<float>(world));
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

    public void Dispose()
    {
        _root.Dispose();
    }
}

public class Group<T> : ISystem<T>
{
    public Arch.CommandBuffer.CommandBuffer CommandBuffer { get; }

    protected readonly List<ISystem<T>> _systems = new();

    public Group(World world)
    {
        CommandBuffer = new Arch.CommandBuffer.CommandBuffer(world);
    }

    public Group<T> Add(params ISystem<T>[] systems)
    {
        _systems.AddRange(systems);

        return this;
    }

    public void Initialize()
    {
        foreach (var system in _systems) {
            if (system is IBufferedSystem bufferedSystem) {
                bufferedSystem.CommandBuffer = CommandBuffer;
            }

            system.Initialize();
        }
    }

    public void BeforeUpdate(in T t)
    {
        OnBeforeUpdate(t);

        foreach (var system in _systems) {
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

        CommandBuffer.Playback();
    }

    public void Dispose()
    {
        foreach (ISystem<T> system in _systems) {
            system.Dispose();
        }

        _systems.Clear();
    }

    protected virtual void OnBeforeUpdate(in T t)
    {
    }
}

public class PresentationGroup<T> : Group<T>
{
    public CommandBuffer? RenderCommandBuffer { get; set; }
    public View? View { get; set; }

    public PresentationGroup(World world) : base(world)
    {
    }

    protected override void OnBeforeUpdate(in T t)
    {
        base.OnBeforeUpdate(in t);

        foreach (var system in _systems) {
            if (system is IPresentationSystem presentationSystem) {
                presentationSystem.RenderCommandBuffer = RenderCommandBuffer;
                presentationSystem.View = View;
            }
        }
    }
}

public interface IBufferedSystem
{
    public Arch.CommandBuffer.CommandBuffer CommandBuffer { get; set; }
}

public interface IPresentationSystem
{
    public CommandBuffer? RenderCommandBuffer { get; set; }
    public View? View { get; set; }
}
