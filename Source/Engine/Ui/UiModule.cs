using System.Collections.Concurrent;
using ChickenWithLips.RmlUi;
using Duck.Content;
using Duck.Graphics;
using Duck.Graphics.Shaders;
using Duck.Input;
using Duck.Logging;
using Duck.Ui.Assets;
using Duck.Ui.Content.ContentLoader;
using Duck.Ui.Content.SourceAssetImporter;
using Duck.Ui.RmlUi;
using RenderInterface = ChickenWithLips.RmlUi.RenderInterface;
using SystemInterface = ChickenWithLips.RmlUi.SystemInterface;

namespace Duck.Ui;

public class UiModule : IUiModule, IInitializableModule, ITickableModule, IShutdownModule
{
    #region Members

    private readonly ILogger _logger;
    private readonly IGraphicsModule _graphicsModule;
    private readonly IContentModule _contentModule;
    private readonly IInputModule _inputModule;

    private SystemInterface? _systemInterface;
    private RenderInterface? _renderInterface;

    private IAsset<ShaderProgram>? _coloredShader;
    private IAsset<ShaderProgram>? _texturedShader;

    private readonly ConcurrentDictionary<string, RmlContext> _contexts = new();
    private readonly ConcurrentDictionary<IAssetReference<UserInterface>, RmlUserInterface> _entityToUserInterface = new();

    #endregion

    #region Methods

    public UiModule(ILogModule logModule, IGraphicsModule graphicsModule, IContentModule contentModule, IInputModule inputModule)
    {
        _graphicsModule = graphicsModule;
        _contentModule = contentModule;
        _inputModule = inputModule;

        _logger = logModule.CreateLogger("UI");
        _logger.LogInformation("Created user interface module.");
    }

    public bool Init()
    {
        _contentModule.RegisterAssetLoader<UserInterface, RmlUserInterface>(new UserInterfaceLoader(this));
        _contentModule.RegisterSourceAssetImporter(new RmlUiAssetImporter());

        _coloredShader = _contentModule.Database.Register(CreateColoredShader());
        _texturedShader = _contentModule.Database.Register(CreateTexturedShader());

        _systemInterface = new RmlUi.SystemInterface(_logger);
        _renderInterface = new RmlUi.RenderInterface(
            _graphicsModule.GraphicsDevice,
            _contentModule,
            (IPlatformAsset<ShaderProgram>)_contentModule.LoadImmediate(_coloredShader.MakeSharedReference()),
            (IPlatformAsset<ShaderProgram>)_contentModule.LoadImmediate(_texturedShader.MakeSharedReference())
        );

        Rml.SetRenderInterface(_renderInterface);
        Rml.SetSystemInterface(_systemInterface);
        Rml.Initialise();

        // FIXME: rml isn't using our content resolver and is looking relative to the root of the project
        Rml.LoadFontFace("Content/Fonts/LatoLatin-Regular.ttf", true);

        return true;
    }

    public void Tick()
    {
        foreach (var kvp in _contexts) {
            var context = kvp.Value;

            if (context.ShouldReceiveInput) {
                context.InjectMouseInput(
                    _inputModule.GetMousePosition(0),
                    _inputModule.IsMouseButtonDown(0),
                    _inputModule.IsMouseButtonDown(1)
                );
            }
        }
    }

    public void Shutdown()
    {
    }

    internal RmlContext CreateContext(string name)
    {
        if (_contexts.ContainsKey(name)) {
            return _contexts[name];
        }

        var context = Rml.CreateContext(name, new Vector2i(1280, 1024));

        if (context == null) {
            throw new Exception("TODO: errors");
        }

        var rmlContext = new RmlContext(context);

        if (!_contexts.TryAdd(name, rmlContext)) {
            throw new Exception("TODO: errors");
        }

        return rmlContext;
    }

    internal void RegisterUserInterface(IAssetReference<UserInterface> assetReference, RmlUserInterface userInterface)
    {
        _entityToUserInterface.TryAdd(assetReference, userInterface);
    }

    internal RmlContext GetContext(string name)
    {
        if (!_contexts.ContainsKey(name)) {
            throw new Exception("TODO: errors");
        }

        return _contexts[name];
    }

    internal RmlUserInterface? GetUserInterface(IAssetReference<UserInterface> assetReference)
    {
        return _entityToUserInterface[assetReference];
    }

    private IAsset<ShaderProgram> CreateColoredShader()
    {
        var fragShader = _contentModule.Database.Register(new FragmentShader(new AssetImportData(new Uri("file:///Shaders/ui-colored.fs"))));
        var vertShader = _contentModule.Database.Register(new VertexShader(new AssetImportData(new Uri("file:///Shaders/ui.vs"))));

        return new ShaderProgram(
            new AssetImportData(new Uri("memory://default-ui-color.shader")),
            vertShader.MakeSharedReference(),
            fragShader.MakeSharedReference()
        );
    }

    private IAsset<ShaderProgram> CreateTexturedShader()
    {
        var fragShader = _contentModule.Database.Register(new FragmentShader(new AssetImportData(new Uri("file:///Shaders/ui-textured.fs"))));
        var vertShader = _contentModule.Database.Register(new VertexShader(new AssetImportData(new Uri("file:///Shaders/ui.vs"))));

        return new ShaderProgram(
            new AssetImportData(new Uri("memory://default-ui-textured.shader")),
            vertShader.MakeSharedReference(),
            fragShader.MakeSharedReference()
        );
    }

    #endregion
}
