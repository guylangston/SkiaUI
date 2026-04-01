using System.Diagnostics.CodeAnalysis;
using SkiaSharp;

namespace SkiaUI.Core;

public interface IPixelTransform
{
    SKPoint Apply(SKPoint p);
    SKPoint Inverse(SKPoint p);
}

public struct PixelTransformOffset : IPixelTransform
{
    SKPoint offset;

    public PixelTransformOffset(SKPoint offset)
    {
        this.offset = offset;
    }

    public SKPoint Apply(SKPoint p) => p + offset;

    public SKPoint Inverse(SKPoint p) => p - offset;
}

public struct PixelTransformCenter : IPixelTransform
{
    SKRect rect;
    SKRect client;

    public PixelTransformCenter(SKRect rect, SKRect client)
    {
        this.rect = rect;
        this.client = client;
    }

    public SKPoint Apply(SKPoint p) => p + new SKPoint(rect.MidX, rect.MidY) - new SKPoint(client.MidX, client.MidY);

    public SKPoint Inverse(SKPoint p) => p + new SKPoint(rect.MidX, rect.MidY);
}


public interface ILayoutOneWay<T>
{
    SKRect Map(T item);
}
public interface ILayout<T> : ILayoutOneWay<T>
{
    T Lookup(int x, int y);
    bool TryLookup(int x, int y, [NotNullWhen(true)] out T? match);
}

public interface ILayoutEnumerable<T> : ILayout<T>
{
    IEnumerable<(T Data, SKRect Location)> ForEach();
}
