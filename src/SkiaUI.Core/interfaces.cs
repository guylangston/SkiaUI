using System.Diagnostics.CodeAnalysis;
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

public interface ISkiaAppCoreLoop
{
    void Paint(SKSurface surface);
    void Step(TimeSpan step);

    void HandleKeyPress(SkiaAppKey key);
    void HandleMousePress(SkiaAppMouse mouse);
}

public interface ISkiaAppAssetFactory
{
    SKPaint GetPaint(string name);
    SKFont GetFont(string? name = null);
}

public interface ISkiaApp : ISkiaAppCoreLoop
{
    TimeSpan Elapsed { get; }
    int FrameCount { get; }

    ISkiaAppAssetFactory AssetFactory { get; }

    bool HandleAppEvent(object app);
    object SendHost(object obj);
}

public interface ISkiaAppExtended : ISkiaApp
{
}

public interface ISkiaAppMainScene : ISkiaAppExtended
{
    IReadOnlyList<ISkiaScene> SceneStack { get; }
    ISkiaScene Active { get; }
    void ScenePush(ISkiaScene scene);
    void ScenePop();
}

public interface ISkiaScene: ISkiaAppCoreLoop
{
    ISkiaApp App { get; }
}



