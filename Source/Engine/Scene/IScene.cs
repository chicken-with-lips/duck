using Arch.Core;
using Duck.Platform;

namespace Duck.Scene;

public interface IScene
{
    public bool IsActive { get; set; }
    public string Name { get; }
    public World World { get; }
    public SceneSystemRoot<FrameTimer> SystemRoot { get; }
}