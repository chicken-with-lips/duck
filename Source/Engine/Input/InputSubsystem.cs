using Duck.Input.Exception;
using Duck.Logging;
using Duck.Platform;
using Silk.NET.Maths;

namespace Duck.Input;

public class InputModule : IInputModule, ITickableModule
{
    #region Members

    private readonly ILogger _logger;
    private readonly IPlatform _platform;

    private readonly Dictionary<string, InputAction> _actions = new();
    private readonly Dictionary<string, InputAxis> _axes = new();

    private readonly float[] _states = new float[(int)InputName.Max];
    private int _mouseX;
    private int _mouseY;

    private bool _isFirstTick = true;

    #endregion

    #region Methods

    public InputModule(ILogModule logModule, IPlatform platform)
    {
        _platform = platform;

        _logger = logModule.CreateLogger("Input");
        _logger.LogInformation("Created input module.");
    }

    public void Tick()
    {
        int mouseDeltaXId = (int)InputName.MouseDeltaX;
        int mouseDeltaYId = (int)InputName.MouseDeltaY;

        _states[mouseDeltaXId] = 0;
        _states[mouseDeltaYId] = 0;


        foreach (var window in _platform.Windows) {
            if (_isFirstTick) {
                var cursorPosition = window?.CursorPosition ?? Vector2D<float>.Zero;

                _mouseX = (int)cursorPosition.X;
                _mouseY = (int)cursorPosition.Y;
                _isFirstTick = false;
            }

            foreach (var windowEvent in window.Events ?? Array.Empty<IWindowEvent>()) {
                if (windowEvent is KeyEvent keyEvent && (int)keyEvent.Key < _states.Length) {
                    _states[(int)keyEvent.Key] = keyEvent.IsDown ? 1 : 0;
                } else if (windowEvent is MousePositionEvent mousePositionEvent) {
                    _states[mouseDeltaXId] = (int)(_mouseX - mousePositionEvent.X);
                    _states[mouseDeltaYId] = (int)(_mouseY - mousePositionEvent.Y);

                    _mouseX = (int)mousePositionEvent.X;
                    _mouseY = (int)mousePositionEvent.Y;
                }
            }
        }

        foreach (var kvp in _axes) {
            UpdateAxis(kvp.Value);
        }

        foreach (var kvp in _actions) {
            UpdateAction(kvp.Value);
        }
    }

    public void Register(InputAction action)
    {
        _actions.Add(action.Name, action);
    }

    public void Register(InputAxis axis)
    {
        _axes.Add(axis.Name, axis);
    }

    public InputAxis GetAxis(string name)
    {
        if (!_axes.ContainsKey(name)) {
            throw new InputNotFoundException();
        }

        return _axes[name];
    }

    public InputAction GetAction(string name)
    {
        if (!_actions.ContainsKey(name)) {
            throw new InputNotFoundException();
        }

        return _actions[name];
    }

    public bool IsKeyDown(InputName input)
    {
        return !IsKeyUp(input);
    }

    public bool IsKeyUp(InputName input)
    {
        return _states[(int)input] == 0;
    }

    private void UpdateAxis(InputAxis axis)
    {
        axis.RawValue = 0;
        axis.Value = 0;

        foreach (var binding in axis.Bindings) {
            var rawValue = GetRawValueForInput(binding.Name);

            if (0 != rawValue) {
                axis.RawValue = rawValue;
                axis.Value = axis.RawValue * binding.Scale;

                break;
            }
        }
    }

    private void UpdateAction(InputAction action)
    {
        action.IsActivated = false;

        foreach (var binding in action.Bindings) {
            if (GetRawValueForInput(binding.Name) != 0) {
                action.IsActivated = true;

                break;
            }
        }
    }

    private float GetRawValueForInput(InputName name)
    {
        var bindingId = (int)name;

        if (bindingId >= _states.Length) {
            return 0f;
        }

        if (bindingId == (int)InputName.Unknown) {
            return 0f;
        }

        if (bindingId <= (int)InputName.Z) {
            return 0 != _states[bindingId] ? 1f : 0f;
        }

        return _states[bindingId];
    }

    #endregion
}
