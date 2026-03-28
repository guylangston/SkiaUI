using SkiaSharp;
using Svg.Skia;

namespace SkiaUI.Tests;


public class SkiaDrawToFile : TestAssetHelper
{
    [Fact]
    public void DrawGridPattern()
    {
        int width = 320;
        int height = 200;
        int gridSpacing = 20;

        using var surface = SKSurface.Create(new SKImageInfo(width, height));
        var canvas = surface.Canvas;

        canvas.Clear(SKColors.White);

        using var paint = new SKPaint
        {
            Color = SKColors.Black,
            StrokeWidth = 1,
            IsAntialias = true
        };

        for (int x = 0; x <= width; x += gridSpacing)
        {
            canvas.DrawLine(x, 0, x, height, paint);
        }

        for (int y = 0; y <= height; y += gridSpacing)
        {
            canvas.DrawLine(0, y, width, y, paint);
        }

        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.OpenWrite(GetPathOutputUsingCallerName(".png"));
        data.SaveTo(stream);
    }

    [Fact]
    public void CanDrawAssets()
    {
        int width = 320;
        int height = 200;

        using var surface = SKSurface.Create(new SKImageInfo(width, height));
        var canvas = surface.Canvas;

        canvas.Clear(SKColors.White);

        // asset
        var assetFile = GetAssetPath("crate.svg");
        var svg = new SKSvg();
        svg.Load(assetFile);

        DrawSvgPic(canvas, svg.Picture, 32, 32, 32, 32);
        DrawSvgPic(canvas, svg.Picture, 64, 64, 32, 32);

        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.OpenWrite(GetPathOutputUsingCallerName(".png"));
        data.SaveTo(stream);

        static  void DrawSvgPic(SKCanvas canvas, SKPicture pic, float x, float y, float width, float height)
        {
            canvas.Save();
            canvas.Translate(x, y);
            // canvas.RotateDegrees((float)shape.Rotation);
            canvas.Scale(width / pic.CullRect.Width, height / pic.CullRect.Height);
            canvas.DrawPicture(pic);
            canvas.Restore();
        }

    }
}
