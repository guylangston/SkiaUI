using Gtk;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Svg.Skia;
using System.IO;

[DllImport("cairo")]
static extern nint cairo_image_surface_create_for_data(nint data, int format, int width, int height, int stride);

[DllImport("cairo")]
static extern void cairo_surface_destroy(nint surface);

[DllImport("cairo")]
static extern void cairo_set_source_surface(nint cr, nint surface, double x, double y);

[DllImport("cairo")]
static extern void cairo_paint(nint cr);

var shapes = new List<Shape>();
var random = new Random();

// Load SVG files
var svgFiles = new[] { "wall.svg", "floor.svg", "crate.svg", "player.svg", "goal.svg" };
var svgImages = new List<SKPicture>();

foreach (var svgFile in svgFiles)
{
    if (File.Exists(svgFile))
    {
        var svg = new SKSvg();
        svg.Load(svgFile);
        if (svg.Picture != null)
        {
            svgImages.Add(svg.Picture);
        }
    }
}

// Create 100 shapes with random SVG images
for (int i = 0; i < 100; i++)
{
    if (svgImages.Count > 0)
    {
        var svgIndex = random.Next(svgImages.Count);
        var svgImage = svgImages[svgIndex];
        var svgBounds = svgImage.CullRect;
        
        shapes.Add(new Shape
        {
            X = random.Next(50, 750),
            Y = random.Next(50, 550),
            VelocityX = random.Next(-5, 6),
            VelocityY = random.Next(-5, 6),
            Size = random.Next(30, 60),
            SvgImage = svgImage,
            SvgBounds = svgBounds,
            Rotation = random.Next(360)
        });
    }
}

double mouseX = 0, mouseY = 0;
var linePoints = new List<(double X, double Y)>();

var app = Gtk.Application.New("com.example.gtk4skia", Gio.ApplicationFlags.DefaultFlags);
app.OnActivate += (sender, args) =>
{
    var window = Gtk.ApplicationWindow.New((Gtk.Application)sender);
    window.Title = "GTK4 SkiaSharp Animation";
    window.SetDefaultSize(800, 600);

    var drawingArea = Gtk.DrawingArea.New();
    
    // Keep surface persistent to avoid recreating every frame
    SKSurface? persistentSurface = null;
    
    Gtk.DrawingAreaDrawFunc drawFunc = (area, cr, width, height) =>
    {
        // Recreate surface only if size changes
        if (persistentSurface == null || 
            persistentSurface.Canvas.DeviceClipBounds.Width != width || 
            persistentSurface.Canvas.DeviceClipBounds.Height != height)
        {
            persistentSurface?.Dispose();
            persistentSurface = SKSurface.Create(new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul));
        }
        
        var canvas = persistentSurface.Canvas;
        canvas.Clear(SKColors.White);
        
        foreach (var shape in shapes)
        {
            if (shape.SvgImage != null)
            {
                canvas.Save();
                
                // Translate to position
                canvas.Translate((float)shape.X, (float)shape.Y);
                
                // Rotate
                canvas.RotateDegrees((float)shape.Rotation);
                
                // Scale to desired size (use cached bounds)
                var scale = (float)shape.Size / Math.Max(shape.SvgBounds.Width, shape.SvgBounds.Height);
                canvas.Scale(scale, scale);
                
                // Center the SVG
                canvas.Translate(-shape.SvgBounds.Width / 2, -shape.SvgBounds.Height / 2);
                
                // Draw the SVG
                canvas.DrawPicture(shape.SvgImage);
                
                canvas.Restore();
            }
        }
        
        // Draw the blue line from points (above the SVGs)
        if (linePoints.Count > 1)
        {
            using var linePaint = new SKPaint
            {
                Color = SKColors.Blue,
                StrokeWidth = 3,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            };
            
            using var path = new SKPath();
            path.MoveTo((float)linePoints[0].X, (float)linePoints[0].Y);
            for (int i = 1; i < linePoints.Count; i++)
            {
                path.LineTo((float)linePoints[i].X, (float)linePoints[i].Y);
            }
            canvas.DrawPath(path, linePaint);
        }
        
        // Draw points as small circles
        if (linePoints.Count > 0)
        {
            using var pointPaint = new SKPaint
            {
                Color = SKColors.Blue,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };
            
            foreach (var point in linePoints)
            {
                canvas.DrawCircle((float)point.X, (float)point.Y, 4, pointPaint);
            }
        }
        
        canvas.Flush();
        
        // Get pixel data
        var pixmap = persistentSurface.PeekPixels();
        if (pixmap != null)
        {
            var pixels = pixmap.GetPixels();
            var stride = pixmap.RowBytes;
            
            // Get the raw Cairo context handle
            var crHandle = cr.Handle.DangerousGetHandle();
            
            // Create Cairo surface from pixel data
            var cairoSurface = cairo_image_surface_create_for_data(pixels, 0, width, height, stride);
            cairo_set_source_surface(crHandle, cairoSurface, 0, 0);
            cairo_paint(crHandle);
            cairo_surface_destroy(cairoSurface);
        }
    };
    
    drawingArea.SetDrawFunc(drawFunc);
    
    var eventController = Gtk.EventControllerMotion.New();
    eventController.OnMotion += (controller, args) => 
    {
        mouseX = args.X;
        mouseY = args.Y;
    };
    drawingArea.AddController(eventController);
    
    // Left click controller for adding line points
    var leftClickController = Gtk.GestureClick.New();
    leftClickController.SetButton(1); // Only left button
    leftClickController.OnPressed += (gesture, args) =>
    {
        linePoints.Add((args.X, args.Y));
    };
    drawingArea.AddController(leftClickController);
    
    // Right click controller for removing line points
    var rightClickController = Gtk.GestureClick.New();
    rightClickController.SetButton(3); // Only right button
    rightClickController.OnPressed += (gesture, args) =>
    {
        double x = args.X;
        double y = args.Y;
        
        // Remove closest point if within 10 pixels
        if (linePoints.Count > 0)
        {
            int closestIndex = -1;
            double closestDistance = double.MaxValue;
            
            for (int i = 0; i < linePoints.Count; i++)
            {
                double dx = x - linePoints[i].X;
                double dy = y - linePoints[i].Y;
                double distance = Math.Sqrt(dx * dx + dy * dy);
                
                if (distance < closestDistance && distance <= 10)
                {
                    closestDistance = distance;
                    closestIndex = i;
                }
            }
            
            if (closestIndex >= 0)
            {
                linePoints.RemoveAt(closestIndex);
            }
        }
    };
    drawingArea.AddController(rightClickController);
    
    // Middle click controller for changing shapes
    var middleClickController = Gtk.GestureClick.New();
    middleClickController.SetButton(2); // Only middle button
    middleClickController.OnPressed += (gesture, args) =>
    {
        double x = args.X;
        double y = args.Y;
        
        foreach (var shape in shapes)
        {
            // Check if click is within shape bounds
            double dx = x - shape.X;
            double dy = y - shape.Y;
            double distance = Math.Sqrt(dx * dx + dy * dy);
            
            if (distance <= shape.Size / 2)
            {
                // Change to a random SVG image
                if (svgImages.Count > 0)
                {
                    var newSvg = svgImages[random.Next(svgImages.Count)];
                    shape.SvgImage = newSvg;
                    shape.SvgBounds = newSvg.CullRect;
                    shape.Rotation = random.Next(360);
                }
            }
        }
    };
    drawingArea.AddController(middleClickController);

    window.SetChild(drawingArea);
    window.Present();

    GLib.Functions.TimeoutAdd(0, 1000 / 60, () =>
    {
        UpdateAnimation();
        drawingArea.QueueDraw();
        return true;
    });
};

void UpdateAnimation()
{
    foreach (var shape in shapes)
    {
        shape.X += shape.VelocityX;
        shape.Y += shape.VelocityY;
        shape.Rotation += 2; // Rotate as they move

        if (shape.X <= 0 || shape.X + shape.Size >= 800)
            shape.VelocityX *= -1;
        if (shape.Y <= 0 || shape.Y + shape.Size >= 600)
            shape.VelocityY *= -1;

        shape.X = Math.Clamp(shape.X, 0, 800 - shape.Size);
        shape.Y = Math.Clamp(shape.Y, 0, 600 - shape.Size);
    }
}

app.RunWithSynchronizationContext(null);

class Shape
{
    public double X { get; set; }
    public double Y { get; set; }
    public double VelocityX { get; set; }
    public double VelocityY { get; set; }
    public int Size { get; set; }
    public SKPicture? SvgImage { get; set; }
    public SKRect SvgBounds { get; set; }
    public double Rotation { get; set; }
}
