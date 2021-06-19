using System;
using System.Collections.Generic;
using Duck.Renderering.Exceptions;
using static GLFWDotNet.GLFW;

namespace Duck.Rendering
{
    public class NativeWindow
    {
        #region Properties

        public IntPtr Ptr => glfwGetX11Window(_nativeWindowPtr);
        public IntPtr Handle => _nativeWindowPtr;

        public bool CloseRequested => glfwWindowShouldClose(_nativeWindowPtr) != 0;
        public int Width { get; private set; }
        public int Height { get; private set; }

        public IWindowEvent[] Events => _events.ToArray();

        #endregion

        #region Members

        private readonly Configuration _config;
        private readonly IntPtr _nativeWindowPtr;
        private readonly List<IWindowEvent> _events = new();

        private readonly GLFWwindowsizefun _onSizeEvent;
        private readonly GLFWkeyfun _onKeyEvent;
        private readonly GLFWcursorposfun _onCursorPosition;

        #endregion

        #region Methods

        public NativeWindow(Configuration config)
        {
            _config = config;
            Width = _config.Width;
            Height = _config.Height;

            glfwWindowHint(GLFW_RESIZABLE, _config.IsResizable ? 1 : 0);

            _nativeWindowPtr = glfwCreateWindow(
                _config.Width,
                _config.Height,
                _config.Title,
                IntPtr.Zero,
                IntPtr.Zero
            );

            if (_nativeWindowPtr == IntPtr.Zero) {
                throw new WindowCreationFailedException();
            }

            _onKeyEvent = new GLFWkeyfun(OnKeyEvent);
            _onSizeEvent = new GLFWwindowsizefun(OnSizeChanged);
            _onCursorPosition = new GLFWcursorposfun(OnCursorPosition);

            glfwSetWindowSizeCallback(_nativeWindowPtr, _onSizeEvent);
            glfwSetKeyCallback(_nativeWindowPtr, _onKeyEvent);
            glfwSetCursorPosCallback(_nativeWindowPtr, _onCursorPosition);
        }

        ~NativeWindow()
        {
            if (_nativeWindowPtr != IntPtr.Zero) {
                glfwDestroyWindow(_nativeWindowPtr);
            }
        }

        public void PumpEvents()
        {
            glfwPollEvents();
        }

        public void ClearEvents()
        {
            _events.Clear();
        }

        private void OnSizeChanged(IntPtr window, int width, int height)
        {
            Width = width;
            Height = height;

            _events.Add(new ResizeEvent() {
                NewWidth = width,
                NewHeight = height,
            });
        }

        private void OnKeyEvent(IntPtr window, int key, int scancode, int action, int mods)
        {
            if (key == GLFW_KEY_UNKNOWN) {
                return;
            }

            _events.Add(new KeyEvent() {
                Key = key,
                Action = action,
            });
        }

        private void OnCursorPosition(IntPtr window, double xPosition, double yPosition)
        {
            _events.Add(new MousePositionEvent() {
                X = xPosition,
                Y = yPosition,
            });
        }

        #endregion


        public struct Configuration
        {
            public int Width;
            public int Height;
            public string Title;

            public bool IsResizable;

            public static Configuration Default => new() {
                Width = 1024,
                Height = 768,
                Title = "Duck",
                IsResizable = true,
            };
        }

        public interface IWindowEvent
        {
        }

        public struct ResizeEvent : IWindowEvent
        {
            public int NewWidth;
            public int NewHeight;
        }

        public struct KeyEvent : IWindowEvent
        {
            public int Key;
            public int Action;
        }

        public struct MousePositionEvent : IWindowEvent
        {
            public double X;
            public double Y;
        }
    }
}
