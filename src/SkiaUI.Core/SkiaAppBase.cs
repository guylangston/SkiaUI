using SkiaSharp;

namespace SkiaUI.Core;

public abstract class SkiaAppBase : SkiaAppCore
{
    protected SkiaAppBase(Func<object, object> hostCallback, ISkiaAppAssetFactory assetFactory) : base(hostCallback, assetFactory)
    {
    }

    public override bool HandleAppEvent(object app)
    {
        WriteLog($"HandleAppEvent {app}");
        return true;
    }

    public override void HandleKeyPress(SkiaAppKey key)
    {
        WriteLog($"HandleKeyPress {key.Key}");
        if (key.Key == "q") SendHost("Quit");
    }

    public override void HandleMousePress(SkiaAppMouse mouse)
    {
        WriteLog($"HandleMousePress X:{mouse.X}, Y:{mouse.Y} Btn:{mouse.Button}[{mouse.Type}]");
    }

}

public abstract class SkiaAppCore : ISkiaApp
{
    int eventCounter;

    protected SkiaAppCore(Func<object, object> hostCallback, ISkiaAppAssetFactory assetFactory)
    {
        HostCallback = hostCallback;
        AssetFactory = assetFactory;
    }

    public Func<object, object> HostCallback { get; }
    public ISkiaAppAssetFactory AssetFactory { get; }
    public TimeSpan Elapsed { get; set; }
    public int FrameCount { get; set; }

    protected void WriteLog(string s)
    {
        Console.WriteLine($"{eventCounter++}:{s}");
    }

    public abstract bool HandleAppEvent(object app);
    public abstract void HandleKeyPress(SkiaAppKey key);
    public abstract void HandleMousePress(SkiaAppMouse mouse);
    public abstract void Paint(SKSurface surface);

    public virtual object SendHost(object obj)
    {
        WriteLog($"SendHost {obj}");
        return HostCallback(obj.ToString());
    }

    public virtual void Step(TimeSpan step)
    {
        FrameCount++;
        Elapsed += step;
    }
}


