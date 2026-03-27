using System;
using Gtk;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.Gtk;
using SkiaUI.Core;

namespace SkiaUI.Gtk;

public class MainWindow : Window
{
    private SKDrawingArea skiaView;
    private ISkiaApp skiaApp;
    private uint timerId;
    private TimeSpan interval;

    public MainWindow(ISkiaApp app) : this(app, new Builder("MainWindow.glade")) { }

    private MainWindow(ISkiaApp app, Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
    {
        if (app is null) throw new ArgumentNullException(nameof(app));
        if (builder is null) throw new ArgumentNullException(nameof(builder));

        this.skiaApp = app;

        this.DeleteEvent += Window_DeleteEvent;
        this.ButtonPressEvent += Window_OnButtonPress;
        this.KeyPressEvent += Window_OnKeyPress;

        builder.Autoconnect(this);

        skiaView               = new SKDrawingArea();
        skiaView.CanFocus      = false;
        skiaView.WidthRequest  = 1920;
        skiaView.HeightRequest = 1080;
        skiaView.PaintSurface += OnPaintSurface;
        skiaView.Show();
        Child = skiaView;

        interval = TimeSpan.FromSeconds(1/60f);
        if (timerId == 0)
        {
            timerId = GLib.Timeout.Add((uint)interval.TotalMilliseconds, OnUpdateTimer);
        }
    }

    SkiaAppMouse Convert(ButtonPressEventArgs args)
    {
        return new SkiaAppMouse()
        {
            X = args.Event.X,
            Y = args.Event.Y,
            Button = args.Event.Button,
            Type = (int)args.Event.State,
            Native = args
        };
    }

    SkiaAppKey Convert(KeyPressEventArgs args)
    {
        return new SkiaAppKey()
        {
            Key = args.Event.Key.ToString(),
            Native = args
        };
    }

    private void Window_OnButtonPress(object o, ButtonPressEventArgs args)
    {
        skiaApp.HandleMousePress(Convert(args));
        args.RetVal = true; // mark event has handled
    }

    private void Window_OnKeyPress(object o, KeyPressEventArgs args)
    {
        skiaApp.HandleKeyPress(Convert(args));
        args.RetVal = true; // mark event has handled
    }

    private void Window_DeleteEvent(object sender, DeleteEventArgs a)
    {
        if (skiaApp.HandleAppEvent("Delete"))
        {
            Application.Quit();
        }
    }

    private bool OnUpdateTimer()
    {
        skiaApp.Step(interval);
        this.QueueDraw();
        return true;
    }

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        skiaApp.Paint(e.Surface);
    }
}
