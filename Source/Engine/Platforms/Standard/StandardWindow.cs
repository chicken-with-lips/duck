using System.Numerics;
using Duck.Platform;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using SilKWindow = Silk.NET.Windowing.Window;
using ISilkWindow = Silk.NET.Windowing.IWindow;
using IWindow = Duck.Platform.IWindow;
using SilkKey = Silk.NET.Input.Key;

namespace Duck.Platforms.Standard;

public class StandardWindow : IWindow
{
    #region Properties

    public int Width => _silkWindow.Size.X;
    public int Height => _silkWindow.Size.Y;

    public Vector2D<int> FrameBufferSize => _silkWindow.FramebufferSize;

    public bool CloseRequested => _silkWindow.IsClosing;
    public Vector2D<float> CursorPosition => _cursorPosition;
    public double ElapsedTime => _silkWindow.Time;
    public IWindowEvent[] Events => _events.ToArray();
    public IntPtr Handle => _silkWindow.Native.Win32.Value.Hwnd;

    public ISilkWindow InternalWindow => _silkWindow;

    #endregion

    #region Members

    private readonly Configuration _config;
    private readonly ISilkWindow _silkWindow;
    private IInputContext? _silkInput;

    private readonly List<IWindowEvent> _events = new();
    private Vector2D<float> _cursorPosition;

    #endregion

    #region Methods

    public StandardWindow(Configuration config)
    {
        _config = config;

        var silkOptions = WindowOptions.Default;
        silkOptions.Size = new Vector2D<int>(_config.Width, _config.Height);
        silkOptions.WindowBorder = _config.IsResizable ? WindowBorder.Resizable : WindowBorder.Fixed;
        silkOptions.Title = _config.Title;
        silkOptions.IsEventDriven = false;
        silkOptions.FramesPerSecond = 60;
        silkOptions.Samples = 2;
        silkOptions.PreferredStencilBufferBits = 8;

        _silkWindow = SilKWindow.Create(silkOptions);

        _silkWindow.Load += () => {
            _silkInput = _silkWindow.CreateInput();

            var keyboard = _silkInput.Keyboards.FirstOrDefault();

            if (keyboard != null) {
                keyboard.KeyDown += (keyboard1, key, arg3) => OnKeyEvent(key, true);
                keyboard.KeyUp += (keyboard1, key, arg3) => OnKeyEvent(key, false);
            }

            var mouse = _silkInput.Mice.FirstOrDefault();

            if (mouse != null) {
                // mouse.Cursor.CursorMode = CursorMode.Raw;
                // mouse.Cursor.IsConfined = true;

                mouse.MouseMove += OnCursorPosition;
                mouse.MouseDown += OnMouseButton;
                mouse.MouseUp += OnMouseButton;

                _cursorPosition = new Vector2D<float>(
                    (int)mouse.Position.X,
                    (int)mouse.Position.Y
                );
            }
        };

        _silkWindow.Resize += OnSizeChanged;
        _silkWindow.Initialize();
    }

    public void Update()
    {
        _silkWindow.DoEvents();
        _silkWindow.DoUpdate();
    }

    public void ClearEvents()
    {
        _events.Clear();
    }

    private void OnSizeChanged(Vector2D<int> newSize)
    {
        _events.Add(new ResizeEvent() {
            NewWidth = newSize.X,
            NewHeight = newSize.Y,
        });
    }

    private void OnKeyEvent(SilkKey key, bool isDown)
    {
        _events.Add(new KeyEvent() {
            Key = (InputName)key,
            IsDown = isDown,
        });
    }

    private void OnCursorPosition(IMouse mouse, Vector2 position)
    {
        _events.Add(new MousePositionEvent() {
            X = position.X,
            Y = position.Y,
        });

        _cursorPosition = new Vector2D<float>(
            mouse.Position.X,
            mouse.Position.Y
        );
    }

    private void OnMouseButton(IMouse mouse, MouseButton button)
    {
        _events.Add(new MouseButtonEvent() {
            ButtonIndex = (int)button,
            IsDown = mouse.IsButtonPressed(button),
        });
    }

    #endregion
}
