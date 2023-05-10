using Arch.System;

namespace Duck.Scene;

public class SystemRoot
{
    public Group<float> InitializationGroup { get; }
    public Group<float> SimulationGroup { get; }
    public Group<float> LateSimulationGroup { get; }
    public Group<float> PresentationGroup { get; }

    private readonly Group<float> _root = new();

    public SystemRoot()
    {
        _root
            .Add(InitializationGroup = new Group<float>())
            .Add(SimulationGroup = new Group<float>())
            .Add(LateSimulationGroup = new Group<float>())
            .Add(PresentationGroup = new Group<float>());
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
