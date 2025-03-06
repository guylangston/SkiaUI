using System;
using SkiaSharp;
using SkiaUI.Core;

namespace SkiaUI.Gtk;

public class SkiaAppExample : ISkiaApp
{
    public TimeSpan Elapsed { get; set; }
    public int FrameCount { get; set; }
    public Func<object, object> HostCallback { get; set; }

    private SKFont fontDefault;
    private SKPaint paintBlack;
    private SKPaint paintWhite;

    public SkiaAppExample()
    {
        paintWhite = new SKPaint()
        {
            Color = SKColors.White,
            Style = SKPaintStyle.Fill
        };
        paintBlack = new SKPaint()
        {
            Color = SKColors.Black,
            Style = SKPaintStyle.Fill
        };

        // https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skfont.-ctor?view=skiasharp-2.88#skiasharp-skfont-ctor(skiasharp-sktypeface-system-single-system-single-system-single).
        var noto = SKTypeface.FromFamilyName("Noto Sans",
                SKFontStyleWeight.Normal,
                SKFontStyleWidth.Normal,
                SKFontStyleSlant.Upright);
        fontDefault = new SKFont(noto, 30);
    }

    public bool HandleAppEvent(object app)
    {
        return true;
    }

    public void HandleKeyPress(SkiaAppKey key)
    {
        if (key.Key == "q") SendHost("Quit");
        Console.WriteLine(key.Key);
    }

    public void HandleMousePress(SkiaAppMouse mouse)
    {
        Console.WriteLine($"X:{mouse.X}, Y:{mouse.Y} Btn:{mouse.Button}[{mouse.Type}]");
    }

    public void Paint(SKSurface surface)
    {
        surface.Canvas.Clear();
        FrameCount++;

        var txt  = $"[Frame {FrameCount}. Elapsed: {Elapsed:hh\\:mm\\:ss} -- Hello World";
        surface.Canvas.DrawText(txt, fontDefault.Size, fontDefault.Size, fontDefault, paintBlack);
    }

    public object SendHost(object obj)
    {
        return HostCallback(obj.ToString());
    }

    public void Step(TimeSpan step)
    {
        Elapsed += step;
    }
}

