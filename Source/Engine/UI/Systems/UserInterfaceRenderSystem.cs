using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using Arch.System.SourceGenerator;
using ChickenWithLips.RmlUi;
using Duck.Renderer;
using Duck.Ui.Components;

namespace Duck.Ui.Systems;

public partial class UserInterfaceRenderSystem : BaseSystem<World, float>, IPresentationSystem
{
    public CommandBuffer? CommandBuffer { get; set; }
    public View? View { get; set; }

    private readonly UiModule _uiModule;

    public UserInterfaceRenderSystem(World world, UiModule uiModule)
        : base(world)
    {
        _uiModule = uiModule;
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Run(in ContextComponent component)
    {
        if (null == CommandBuffer || null == View) {
            return;
        }

        var context = _uiModule.FindContext(component.Name);

        if (null == context) {
            return;
        }

        context.Context.Dimensions = new Vector2i(View.Dimensions.X, View.Dimensions.Y);
        _uiModule.RenderInterface.CommandBuffer = CommandBuffer;

        context.Render();
    }
}
