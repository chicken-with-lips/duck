using System;
using System.Collections.Generic;
using Duck.Contracts;
using Duck.Contracts.Input;
using Duck.Contracts.Logging;
using Duck.Rendering;
using GLFWDotNet;
using static GLFWDotNet.GLFW;

namespace Duck.Input
{
    public class InputSubsystem : IInputSubsystem, IApplicationInitializableSubsystem, IApplicationTickableSubsystem
    {
        #region Members

        private readonly ILogSubsystem _logSubsystem;
        private readonly RenderingSubsystem _renderingSubsystem;

        private readonly Dictionary<string, InputAction> _actions = new();
        private readonly Dictionary<string, InputAxis> _axes = new();

        private readonly float[] _states = new float[(int) InputName.Max];
        private int _mouseX;
        private int _mouseY;

        private bool _isInitialized;
        private bool _isFirstTick = true;
        private ILogger? _logger;

        #endregion

        #region Methods

        internal InputSubsystem(ILogSubsystem logSubsystem, RenderingSubsystem renderingSubsystem)
        {
            _logSubsystem = logSubsystem;
            _renderingSubsystem = renderingSubsystem;
        }

        public bool Init()
        {
            if (_isInitialized) {
                throw new Exception("InputSubsystem has already been initialized");
            }

            _logger = _logSubsystem.CreateLogger("Input");
            _logger.LogInformation("Initializing input subsystem.");

            glfwSetInputMode(_renderingSubsystem.NativeWindow.Handle, GLFW_CURSOR, GLFW_CURSOR_DISABLED);

            _isInitialized = true;

            return true;
        }

        public void Tick()
        {
            int mouseDeltaXId = (int) InputName.MouseDeltaX;
            int mouseDeltaYId = (int) InputName.MouseDeltaY;

            _states[mouseDeltaXId] = 0;
            _states[mouseDeltaYId] = 0;

            if (_isFirstTick) {
                glfwGetCursorPos(_renderingSubsystem.NativeWindow.Handle, out var x, out var y);

                _mouseX = (int) x;
                _mouseY = (int) y;
                _isFirstTick = false;
            }

            foreach (var windowEvent in _renderingSubsystem.NativeWindow.Events) {
                if (windowEvent is NativeWindow.KeyEvent keyEvent && keyEvent.Key < _states.Length) {
                    _states[keyEvent.Key] = keyEvent.Action;
                } else if (windowEvent is NativeWindow.MousePositionEvent mousePositionEvent) {
                    _states[mouseDeltaXId] = (int) (_mouseX - mousePositionEvent.X);
                    _states[mouseDeltaYId] = (int) (_mouseY - mousePositionEvent.Y);

                    _mouseX = (int) mousePositionEvent.X;
                    _mouseY = (int) mousePositionEvent.Y;
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
            var bindingId = (int) name;

            if (bindingId >= _states.Length) {
                return 0f;
            }

            if (bindingId <= GLFW_KEY_Z) {
                return 0 != _states[bindingId] ? 1f : 0f;
            }

            return _states[bindingId];
        }

        #endregion
    }
}
