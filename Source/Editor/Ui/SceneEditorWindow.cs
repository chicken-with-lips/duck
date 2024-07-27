using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using Duck;
using Duck.Content;
using Duck.Graphics;
using Duck.Platform;
using Duck.Serialization;
using Duck.Ui;
using Duck.Ui.Elements;
using Editor.Modules;

namespace Editor.Ui;

[AutoSerializable]
public partial struct SceneEditorWindowComponent
{
}

public partial class SceneEditorWindow : BaseSystem<World, float>
{
    private readonly IScene _scene;
    private readonly IUIModule _uiModule;
    private readonly IContentModule _contentModule;
    private readonly IApplication _app;
    private readonly GameHostModule _gameHost;
    private readonly Action _onPlayClicked;

    public SceneEditorWindow(World world, IScene scene, IUIModule uiModule, IContentModule contentModule, IApplication app, GameHostModule gameHost)
        : base(world)
    {
        _scene = scene;
        _uiModule = uiModule;
        _contentModule = contentModule;
        _app = app;
        _gameHost = gameHost;

        _onPlayClicked = OnPlayClicked;
    }

    [Query]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Run(in SceneEditorWindowComponent sceneEditorWindow)
    {
        var c = _uiModule.GetContextForScene(_scene);

        c.New(
            RootProps.Default with {
                Font = _contentModule.Database.GetAsset<Font>(new Uri("file:///Builtin/Fonts/Manrope/Medium.arfont")).MakeSharedReference(),
            },
            c.HorizontalContainer(
                HorizontalContainerProps.Default,
                c.Button(
                    ButtonProps.Default with {
                        Box = Box.Default with {
                            ContentHeight = 2,
                            ContentWidth = 3,
                        },
                        BackgroundColor = Color.Gray,
                    },
                    c.Label(
                        LabelProps.Default with {
                            Content = _app.IsInPlayMode ? "Stop" : "Play"
                        }
                    ),
                    _onPlayClicked
                )
            ),
            c.HorizontalContainer(
                HorizontalContainerProps.Default with {
                    GapSize = 0.5f,
                },
                c.VerticalContainer(
                    VerticalContainerProps.Default,
                    c.Panel(
                        PanelProps.Default with {
                            BackgroundColor = Color.Blue,
                            Box = Box.Default with {
                                ContentWidth = 18,
                                ContentHeight = 100
                            }
                        }
                    )
                ),
                c.VerticalContainer(
                    VerticalContainerProps.Default with {
                        GapSize = 0.5f,
                    },
                    c.RenderView(
                        RenderViewProps.Default with {
                            Box = Box.Default with {
                                ContentWidth = 44,
                                ContentHeight = 44
                            },
                            View = _gameHost.GetModule<IRendererModule>().PrimaryView,
                        }
                    ),
                    c.Panel(
                        PanelProps.Default with {
                            BackgroundColor = Color.Red,
                            Box = Box.Default with {
                                ContentWidth = 44,
                                ContentHeight = 20
                            }
                        }
                    )
                ),
                c.VerticalContainer(
                    VerticalContainerProps.Default,
                    c.Panel(
                        PanelProps.Default with {
                            BackgroundColor = Color.Blue,
                            Box = Box.Default with {
                                ContentWidth = 18,
                                ContentHeight = 100
                            }
                        }
                    )
                )
            )
        );
    }

    private void OnPlayClicked()
    {
        if (_gameHost.InstancedApplication.IsInPlayMode) {
            _app.ExitPlayMode();
        } else {
            _app.EnterPlayMode();
        }
    }
}
