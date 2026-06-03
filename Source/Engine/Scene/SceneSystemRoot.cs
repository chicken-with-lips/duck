using System.Reflection;
using Arch.Buffer;
using Arch.Core;
using Arch.System;

namespace Duck.Scene;

public class SceneSystemRoot<T> : IDisposable
{
    public Group<T> InitializationGroup { get; }
    public Group<T> SimulationGroup { get; }
    public Group<T> FixedSimulationGroup { get; }
    public Group<T> EarlySimulationGroup { get; }
    public Group<T> LateSimulationGroup { get; }
    public Group<T> PresentationGroup { get; }
    public Group<T> ExitFrameGroup { get; }

    private readonly Group<T> _root;

    public SceneSystemRoot(World world)
    {
        _root = new(world);

        _root
            .Add(InitializationGroup = new Group<T>(world))
            .Add(EarlySimulationGroup = new Group<T>(world))
            .Add(SimulationGroup = new Group<T>(world))
            .Add(FixedSimulationGroup = new Group<T>(world))
            .Add(LateSimulationGroup = new Group<T>(world))
            .Add(PresentationGroup = new Group<T>(world))
            .Add(ExitFrameGroup = new Group<T>(world));
    }

    public void RemoveByAssembly(Assembly assembly)
    {
        _root.RemoveByAssembly(assembly);
    }

    public void Initialize()
    {
        _root.Initialize();
    }

    public void BeforeUpdate(in T t)
    {
        _root.BeforeUpdate(t);
    }

    public void Update(in T t)
    {
        _root.Update(t);
    }

    public void AfterUpdate(in T t)
    {
        _root.AfterUpdate(t);
    }

    public void Dispose()
    {
        _root.Dispose();
    }
}

public class Group<T> : ISystem<T>
{
    public CommandBuffer CommandBuffer { get; }
    internal List<ISystem<T>> Systems => _systems;

    private readonly List<ISystem<T>> _systems = new();

    private World _world;
    private bool _isInitialized;

    public Group(World world)
    {
        _world = world;
        CommandBuffer = new CommandBuffer();
    }

    public Group<T> Add(params ISystem<T>[] systems)
    {
        _systems.AddRange(systems);

        if (_isInitialized) {
            InitializeSystems(systems);
        }

        return this;
    }

    public void RemoveByAssembly(Assembly assembly)
    {
        var toRemove = new List<ISystem<T>>();

        foreach (var system in _systems) {
            if (system is Group<T> group) {
                group.RemoveByAssembly(assembly);
            } else if (system.GetType().Assembly.Equals(assembly)) {
                toRemove.Add(system);
            }
        }

        toRemove.ForEach(x => _systems.Remove(x));
    }

    public void Initialize()
    {
        InitializeSystems(_systems.ToArray());

        _isInitialized = true;
    }

    private void InitializeSystems(ISystem<T>[] systems)
    {
        foreach (var system in systems) {
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

        CommandBuffer.Playback(_world);
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

public interface IBufferedSystem
{
    public CommandBuffer? CommandBuffer { get; set; }
}
