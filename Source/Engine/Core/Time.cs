using Duck.Platform;

namespace Duck;

public static class Time
{
    public static float DeltaFrame => FrameTimer?.Delta ?? 0;
    public static double DoubleDeltaFrame => FrameTimer?.DoubleDelta ?? 0;

    public static IFrameTimer? FrameTimer { get; set; }
}
