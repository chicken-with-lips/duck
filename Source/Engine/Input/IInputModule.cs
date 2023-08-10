using Silk.NET.Maths;

namespace Duck.Input;

public interface IInputModule : IModule
{
    public int MaxSupportedMouseButtons { get; }

    public void Register(InputAction action);
    public void Register(InputAxis axis);

    public InputAxis GetAxis(string name);
    public InputAction GetAction(string name);
    bool IsKeyDown(InputName input);
    bool IsKeyUp(InputName input);

    public Vector2D<int> GetMousePosition(int index);
    public bool IsMouseButtonDown(int index);
    public bool WasMouseButtonDown(int index);
}
