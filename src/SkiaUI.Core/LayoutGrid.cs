using System.Diagnostics.CodeAnalysis;
using SkiaSharp;

namespace SkiaUI.Core;

public class LayoutGrid : ILayoutEnumerable<(int X, int Y)>
{
    public int CellWidth { get; set; }
    public int CellHeight { get; set; }
    public int GridWidth { get; set; }
    public int GridHeight { get; set; }

    public LayoutGrid(int cellWidth, int cellHeight, int gridWidth, int gridHeight)
    {
        CellWidth = cellWidth;
        CellHeight = cellHeight;
        GridWidth = gridWidth;
        GridHeight = gridHeight;
    }

    public SKRect Region => new SKRect(0,0, GridWidth * CellWidth, GridHeight * CellHeight);

    public IEnumerable<((int X, int Y) Data, SKRect Location)> ForEach()
    {
        for(var cy=0; cy<GridWidth; cy++)
        {
            for(var cx=0; cx<GridWidth; cx++)
            {
                yield return ( (cx,cy), Map((cx, cy)) );
            }
        }
    }

    public (int X, int Y) Lookup(int x, int y)
    {
        if (TryLookup(x, y, out var match))
        {
            return match;
        }
        throw new ArgumentOutOfRangeException("x,y");
    }

    public SKRect Map((int X, int Y) item)
    {
        return new SKRect(item.X * CellWidth, item.Y * CellHeight, item.X * CellWidth + CellWidth, item.Y * CellHeight + CellHeight);
    }

    public bool TryLookup(int x, int y, [NotNullWhen(true)] out (int X, int Y) match)
    {
        match = (x / CellWidth, y / CellHeight);
        if (match.X < 0 || match.X > GridWidth) return false;
        if (match.Y < 0 || match.Y > GridHeight) return false;
        return true;
    }
}

