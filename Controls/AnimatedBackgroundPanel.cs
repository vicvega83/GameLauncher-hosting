using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace GameLauncher.Controls;

public class AnimatedBackgroundPanel : Panel
{
    private Image? _backgroundImage;
    private double _scale = 1.0;
    private double _targetScale;
    private double _offsetX;
    private double _targetOffsetX;
    private double _offsetY;
    private double _targetOffsetY;
    private System.Windows.Forms.Timer _animateTimer;
    private double _time = 0;
    private double _zoomSpeed;
    private double _panSpeed;
    private double _zoomMin;
    private double _zoomMax;

    public AnimatedBackgroundPanel()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.UserPaint |
            ControlStyles.ResizeRedraw,
            true);
        DoubleBuffered = true;
        BackColor = Color.Black;

        _animateTimer = new System.Windows.Forms.Timer();
        _animateTimer.Interval = 16;
        _animateTimer.Tick += AnimateTimer_Tick;
        _animateTimer.Start();
    }

    [Browsable(false)]
    public new Image? BackgroundImage
    {
        get => _backgroundImage;
        set
        {
            _backgroundImage = value;
            if (value != null)
            {
                RecalculateTargets();
                Invalidate();
            }
        }
    }

    public double ZoomSpeed
    {
        set => _zoomSpeed = value;
    }

    public double PanSpeed
    {
        set => _panSpeed = value;
    }

    public double ZoomMin
    {
        set => _zoomMin = value;
    }

    public double ZoomMax
    {
        set => _zoomMax = value;
    }

    private void RecalculateTargets()
    {
        if (_backgroundImage == null || Width == 0 || Height == 0) return;

        double scaleX = (double)Width / _backgroundImage.Width;
        double scaleY = (double)Height / _backgroundImage.Height;
        double baseScale = Math.Max(scaleX, scaleY);

        _targetScale = baseScale * 1.05;
    }

    private void AnimateTimer_Tick(object? sender, EventArgs e)
    {
        _time += 0.016;

        double sineValue = Math.Sin(_time * _panSpeed * 100);
        _targetOffsetX = sineValue * (_backgroundImage != null ? _backgroundImage.Width * 0.05 : 50);
        _targetOffsetY = Math.Cos(_time * _panSpeed * 80) * (_backgroundImage != null ? _backgroundImage.Height * 0.03 : 30);

        _targetScale = 1.0 + Math.Sin(_time * _zoomSpeed * 50) * ((_zoomMax - _zoomMin) / 2.0);

        _offsetX += (_targetOffsetX - _offsetX) * 0.02;
        _offsetY += (_targetOffsetY - _offsetY) * 0.02;
        _scale += (_targetScale - _scale) * 0.02;

        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs pevent)
    {
        base.OnPaint(pevent);

        if (_backgroundImage == null)
        {
            using SolidBrush brush = new(BackColor);
            pevent.Graphics.FillRectangle(brush, ClientRectangle);
            return;
        }

        using Graphics g = pevent.Graphics;
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

        double scaledWidth = _backgroundImage.Width * _scale;
        double scaledHeight = _backgroundImage.Height * _scale;

        double destX = -_offsetX + (Width - scaledWidth) / 2.0;
        double destY = -_offsetY + (Height - scaledHeight) / 2.0;

        RectangleF destRect = new(
            (float)destX,
            (float)destY,
            (float)scaledWidth,
            (float)scaledHeight);

        g.DrawImage(_backgroundImage, destRect);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        RecalculateTargets();
        Invalidate();
    }
}
