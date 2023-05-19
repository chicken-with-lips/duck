using Arch.Core;
using Duck.Serialization;

namespace Duck.Renderer;

[AutoSerializable]
public partial interface IScene
{
    public bool IsActive { get; set; }

    public string Name { get; }
    public World World { get; }
    public SystemRoot SystemRoot { get; }

    public void PreTick(in float deltaTime);
    public void Tick(in float deltaTime);
    public void PostTick(in float deltaTime);
}