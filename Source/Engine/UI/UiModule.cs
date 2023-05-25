using System.Collections.Concurrent;
using ChickenWithLips.RmlUi;
using Duck.Content;
using Duck.Input;
using Duck.Logging;
using Duck.Renderer;
using Duck.Renderer.Materials;
using Duck.Renderer.Shaders;
using Duck.Ui.Assets;
using Duck.Ui.Content.ContentLoader;
using Duck.Ui.Content.SourceAssetImporter;
using Duck.Ui.RmlUi;
using RenderInterface = ChickenWithLips.RmlUi.RenderInterface;
using SystemInterface = ChickenWithLips.RmlUi.SystemInterface;

namespace Duck.Ui;

public class UiModule : IUiModule, IInitializableModule, ITickableModule, IShutdownModule
{
    #region Properties

    internal RmlUi.RenderInterface? RenderInterface => _renderInterface;

    #endregion

    #region Members

    private readonly ILogger _logger;
    private readonly IRendererModule _renderModule;
    private readonly IContentModule _contentModule;
    private readonly IInputModule _inputModule;

    private RmlUi.SystemInterface? _systemInterface;
    private RmlUi.RenderInterface? _renderInterface;

    private IAsset<Material>? _coloredMaterial;
    private IAsset<Material>? _texturedMaterial;

    private readonly ConcurrentDictionary<string, RmlContext> _contexts = new();
    private readonly ConcurrentDictionary<IAssetReference<UserInterface>, RmlUserInterface> _entityToUserInterface = new();

    #endregion

    #region Methods

    public UiModule(ILogModule logModule, IRendererModule renderModule, IContentModule contentModule, IInputModule inputModule)
    {
        _renderModule = renderModule;
        _contentModule = contentModule;
        _inputModule = inputModule;

        _logger = logModule.CreateLogger("UI");
        _logger.LogInformation("Created user interface module.");
    }

    public bool Init()
    {
        _contentModule.RegisterAssetLoader<UserInterface, RmlUserInterface>(new UserInterfaceLoader(this, _logger));
        _contentModule.RegisterSourceAssetImporter(new RmlUiAssetImporter());

        CreateShaders();

        _systemInterface = new RmlUi.SystemInterface(_logger);
        _renderInterface = new RmlUi.RenderInterface(
            _renderModule.GraphicsDevice,
            _contentModule,
            (IPlatformAsset<Material>)_contentModule.LoadImmediate(_coloredMaterial.MakeSharedReference()),
            (IPlatformAsset<Material>)_contentModule.LoadImmediate(_texturedMaterial.MakeSharedReference())
        );

        Rml.SetRenderInterface(_renderInterface);
        Rml.SetSystemInterface(_systemInterface);
        Rml.Initialise();

        // FIXME: rml isn't using our content resolver and is looking relative to the root of the project
        Rml.LoadFontFace(_contentModule.ContentRootDirectory + "/Fonts/LatoLatin-Regular.ttf", true);

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
        _contexts.Clear();

        Rml.Shutdown();
    }

    internal RmlContext CreateContext(string name)
    {
        if (_contexts.ContainsKey(name)) {
            return _contexts[name];
        }

        // FIXME: 1280x1024
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

    internal RmlContext? FindContext(string name)
    {
        if (_contexts.TryGetValue(name, out var context)) {
            return context;
        }

        return null;
    }

    internal RmlContext GetOrCreateContext(string name)
    {
        if (!_contexts.TryGetValue(name, out var context)) {
            context = CreateContext(name);
        }

        return context;
    }

    internal RmlUserInterface? GetUserInterface(IAssetReference<UserInterface> assetReference)
    {
        return _entityToUserInterface[assetReference];
    }

    private void CreateShaders()
    {
        var coloredFragShader = _contentModule.Database.Register(new FragmentShader(new AssetImportData(new Uri("file:///Builtin/Shaders/ui-colored.frag"))));
        var texturedFragShader = _contentModule.Database.Register(new FragmentShader(new AssetImportData(new Uri("file:///Builtin/Shaders/ui-textured.frag"))));
        var vertShader = _contentModule.Database.Register(new VertexShader(new AssetImportData(new Uri("file:///Builtin/Shaders/ui.vert"))));

        var coloredShader = _contentModule.Database.Register(
            new ShaderProgram(
                new AssetImportData(new Uri("memory://default-ui-color.shader")),
                vertShader.MakeSharedReference(),
                coloredFragShader.MakeSharedReference()
            )
        );

        var texturedShader = _contentModule.Database.Register(
            new ShaderProgram(
                new AssetImportData(new Uri("memory://default-ui-textured.shader")),
                vertShader.MakeSharedReference(),
                texturedFragShader.MakeSharedReference()
            )
        );

        var mat = new Material(
            new AssetImportData(new Uri("memory:///ui/colored.mat"))
        );
        mat.Shader = coloredShader.MakeSharedReference();

        _coloredMaterial = _contentModule.Database.Register(mat);

        mat = new Material(
            new AssetImportData(new Uri("memory:///ui/textured.mat"))
        );
        mat.Shader = texturedShader.MakeSharedReference();

        _texturedMaterial = _contentModule.Database.Register(mat);
    }

    #endregion
}
