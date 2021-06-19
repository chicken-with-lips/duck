using GLFWDotNet;

namespace Duck.Timing
{
    public class FrameTimer
    {
        #region Properties

        public float Delta { get; private set; }
        public double DoubleDelta { get; private set; }

        #endregion

        #region Members

        private double _previousTime;

        #endregion

        #region Methods

        public FrameTimer()
        {
            _previousTime = GLFW.glfwGetTime();
        }

        public void Update()
        {
            var time = GLFW.glfwGetTime();

            DoubleDelta = time - _previousTime;
            Delta = (float) DoubleDelta;

            _previousTime = time;
        }

        #endregion
    }
}
