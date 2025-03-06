using SkiaSharp;

namespace SkiaUI.Core;

public struct SkiaAppKey
{
    public string Key { get; set; }
    public object Native { get; set; }
}

public struct SkiaAppMouse
{
    public double X { get; set; }
    public double Y { get; set; }
    public int Type { get; set; }
    public uint Button { get; set; }
    public object Native { get; set; }
}

public interface ISkiaApp
{
    TimeSpan Elapsed { get; }
    int FrameCount { get; }

    void Paint(SKSurface surface);
    void Step(TimeSpan step);

    void HandleKeyPress(SkiaAppKey key);
    void HandleMousePress(SkiaAppMouse mouse);

    bool HandleAppEvent(object app);
    object SendHost(object obj);
}
