using Duck.Timing;

namespace Duck
{
    public static class Time
    {
        public static float DeltaFrame => InternalFrameTimer.Delta;
        public static double DoubleDeltaFrame => InternalFrameTimer.DoubleDelta;

        internal static FrameTimer InternalFrameTimer { get; set; }
    }
}
