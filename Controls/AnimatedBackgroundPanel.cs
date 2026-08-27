using System;
using System.Drawing;
using System.Windows.Forms;

namespace GameLauncher.Controls;

public class AnimatedBackgroundPanel : Panel
{
    private Image? _backgroundImage;
    private System.Windows.Forms.Timer? _animateTimer;
    private int _tick;
    private double _zoomSpeed;
    private double _panSpeed;
    private double _zoomMin;
    private double _zoomMax;

    public AnimatedBackgroundPanel()
    {
        BackColor = Color.Black;
    }

    public Image? BackgroundImage
    {
        get => _backgroundImage;
        set => _backgroundImage = value;
    }

    public double ZoomSpeed { set => _zoomSpeed = value; }
    public double PanSpeed { set => _panSpeed = value; }
    public double ZoomMin { set => _zoomMin = value; }
    public double ZoomMax { set => _zoomMax = value; }

    protected override void OnPaint(PaintEventArgs pe)
    {
        pe.Graphics.Clear(BackColor);
        
        if (_backgroundImage != null)
        {
            pe.Graphics.DrawImage(_backgroundImage, 0, 0, Width, Height);
        }
    }
}
