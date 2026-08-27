using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.IO.Compression;
using System.Text.Json;
using System.Windows.Forms;
using GameLauncher.Controls;
using GameLauncher.Models;
using GameLauncher.Services;

namespace GameLauncher;

public partial class LauncherForm : Form
{
    private AnimatedBackgroundPanel? _backgroundPanel;
    private PatchNotesPanel? _patchNotesPanel;
    private Panel? _bottomPanel;
    private Button? _playButton;
    private Label? _versionLabel;
    private Label? _statusLabel;
    private Panel? _overlayPanel;
    private Panel? _leftOverlay;
    private System.Windows.Forms.Timer? _fadeInTimer;
    private Panel? _downloadPanel;
    private ProgressBar? _downloadProgress;
    private Label? _downloadLabel;

    private DataService _dataService = new();
    private LauncherConfig? _config;
    private VersionInfo? _remoteVersion;
    private string _installedVersion = "0.0.0";
    private bool _isDownloading = false;

    public LauncherForm()
    {
        InitializeComponent();
        SetupUI();
        Load += LauncherForm_Load;
    }

    private void InitializeComponent()
    {
        this.SuspendLayout();
        this.BackColor = Color.Black;
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MinimumSize = new Size(1200, 700);
        this.MaximumSize = new Size(1920, 1080);
        this.Size = new Size(1920, 1080);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Text = "GameLauncher";
        this.ResumeLayout(false);
    }

    private void SetupUI()
    {
        _backgroundPanel = new AnimatedBackgroundPanel();
        _backgroundPanel.Dock = DockStyle.Fill;
        _backgroundPanel.BringToFront();
        Controls.Add(_backgroundPanel!);

        _overlayPanel = new Panel();
        _overlayPanel.Dock = DockStyle.Fill;
        _overlayPanel.BackColor = Color.FromArgb(60, 0, 0, 0);
        _overlayPanel.BringToFront();
        Controls.Add(_overlayPanel!);

        _leftOverlay = new Panel();
        _leftOverlay.Dock = DockStyle.Left;
        _leftOverlay.Width = 80;
        _leftOverlay.BackColor = Color.FromArgb(80, 0, 0, 0);
        _overlayPanel!.Controls.Add(_leftOverlay!);

        _patchNotesPanel = new PatchNotesPanel();
        _patchNotesPanel!.BringToFront();
        _overlayPanel!.Controls.Add(_patchNotesPanel!);

        _bottomPanel = new Panel();
        _bottomPanel!.Dock = DockStyle.Bottom;
        _bottomPanel.Height = 100;
        _bottomPanel.BackColor = Color.FromArgb(0, 0, 0, 0);
        _overlayPanel!.Controls.Add(_bottomPanel!);

        _playButton = new Button();
        _playButton!.Location = new Point((_bottomPanel!.Width - 200) / 2, 15);
        _playButton.Size = new Size(200, 50);
        _playButton.Text = "Loading...";
        _playButton.Font = new Font("Segoe UI", 16, FontStyle.Bold);
        _playButton.BackColor = Color.FromArgb(0, 120, 215);
        _playButton.ForeColor = Color.White;
        _playButton.FlatStyle = FlatStyle.Flat;
        _playButton.FlatAppearance.BorderSize = 0;
        _playButton.Cursor = Cursors.Hand;
        _playButton.Click += PlayButton_Click;
        _playButton.Paint += PlayButton_Paint;
        _bottomPanel!.Controls.Add(_playButton!);

        _versionLabel = new Label();
        _versionLabel!.Location = new Point((_bottomPanel!.Width - 200) / 2, 70);
        _versionLabel.Size = new Size(200, 20);
        _versionLabel.Text = "Checking version...";
        _versionLabel.Font = new Font("Segoe UI", 9);
        _versionLabel.ForeColor = Color.FromArgb(150, 150, 150);
        _versionLabel.TextAlign = ContentAlignment.TopCenter;
        _bottomPanel!.Controls.Add(_versionLabel!);

        _statusLabel = new Label();
        _statusLabel!.Location = new Point(30, 40);
        _statusLabel.Size = new Size(400, 60);
        _statusLabel.Text = "";
        _statusLabel.Font = new Font("Segoe UI", 14, FontStyle.Bold);
        _statusLabel.ForeColor = Color.White;
        _overlayPanel!.Controls.Add(_statusLabel!);

        _downloadPanel = new Panel();
        _downloadPanel!.Dock = DockStyle.Bottom;
        _downloadPanel.Height = 60;
        _downloadPanel.BackColor = Color.FromArgb(40, 40, 40, 40);
        _downloadPanel.Visible = false;
        _overlayPanel!.Controls.Add(_downloadPanel!);

        _downloadProgress = new ProgressBar();
        _downloadProgress!.Location = new Point(20, 10);
        _downloadProgress.Size = new Size(_downloadPanel!.Width - 240, 20);
        _downloadProgress.Style = ProgressBarStyle.Continuous;
        _downloadProgress.ForeColor = Color.FromArgb(0, 120, 215);
        _downloadPanel!.Controls.Add(_downloadProgress!);

        _downloadLabel = new Label();
        _downloadLabel!.Location = new Point(_downloadPanel!.Width - 150, 12);
        _downloadLabel.Size = new Size(130, 20);
        _downloadLabel.Text = "0%";
        _downloadLabel.Font = new Font("Segoe UI", 10);
        _downloadLabel.ForeColor = Color.White;
        _downloadLabel.TextAlign = ContentAlignment.MiddleRight;
        _downloadPanel!.Controls.Add(_downloadLabel!);

        _fadeInTimer = new System.Windows.Forms.Timer();
        _fadeInTimer!.Interval = 50;
        _fadeInTimer.Tick += FadeInTimer_Tick;
    }

    private async void LauncherForm_Load(object? sender, EventArgs e)
    {
        SetDefaultBackground();
        LoadEmbeddedConfig();

        await Task.Delay(300);
        _fadeInTimer!.Start();

        await InitializeLauncherAsync();
    }

    private void SetDefaultBackground()
    {
        if (_backgroundPanel == null) return;
        Bitmap defaultBg = CreateDefaultBackground();
        _backgroundPanel.BackgroundImage = defaultBg;
    }

    private Bitmap CreateDefaultBackground()
    {
        int width = 1920;
        int height = 1080;
        Bitmap bmp = new(width, height);

        using Graphics g = Graphics.FromImage(bmp);

        LinearGradientBrush gradient = new(
            new Point(0, 0),
            new Point(width, height),
            Color.FromArgb(20, 20, 40),
            Color.FromArgb(5, 5, 15));
        g.FillRectangle(gradient, new Rectangle(0, 0, width, height));

        using SolidBrush brush = new(Color.FromArgb(30, 50, 80));
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                int x = (i * 400) + 100;
                int y = (j * 400) + 200;
                using SolidBrush subBrush = new(Color.FromArgb(15 + i * 5, 25 + j * 10, 50 + i * 5));
                g.FillEllipse(subBrush, x, y, 200, 200);
            }
        }

        return bmp;
    }

    private void LoadEmbeddedConfig()
    {
        string? embeddedConfig = GetEmbeddedResource("GameLauncher.config.launcher.json");

        if (!string.IsNullOrEmpty(embeddedConfig))
        {
            _config = JsonSerializer.Deserialize<LauncherConfig>(embeddedConfig, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (_config != null && !string.IsNullOrEmpty(_config.BackgroundUrl))
            {
                LoadBackgroundFromUrl(_config.BackgroundUrl);
            }
        }
        else
        {
            _config = new LauncherConfig
            {
                GameName = "Game",
                GameExe = "Game.exe",
                ZoomSpeed = 0.00015,
                PanSpeed = 0.0001,
                ZoomMin = 1.0,
                ZoomMax = 1.3
            };
        }

        if (_config != null && _backgroundPanel != null)
        {
            _backgroundPanel.ZoomSpeed = _config.ZoomSpeed;
            _backgroundPanel.PanSpeed = _config.PanSpeed;
            _backgroundPanel.ZoomMin = _config.ZoomMin;
            _backgroundPanel.ZoomMax = _config.ZoomMax;
        }
    }

    private async void LoadBackgroundFromUrl(string url)
    {
        try
        {
            byte[]? data = await _dataService.DownloadFile(url);
            if (data != null && data.Length > 0 && _backgroundPanel != null)
            {
                using MemoryStream ms = new(data);
                Image img = Image.FromStream(ms);
                _backgroundPanel.BackgroundImage = img;
            }
        }
        catch
        {
            SetDefaultBackground();
        }
    }

    private async Task InitializeLauncherAsync()
    {
        if (_config == null) return;

        _statusLabel!.Text = _config.GameName;

        await LoadVersionAsync();
        await LoadPatchNotesAsync();
        UpdatePlayButton();
    }

    private async Task LoadVersionAsync()
    {
        if (string.IsNullOrEmpty(_config?.VersionUrl))
        {
            _versionLabel!.Text = "No version config";
            return;
        }

        _remoteVersion = await _dataService.LoadVersionInfo(_config.VersionUrl);

        if (_remoteVersion != null)
        {
            _installedVersion = GetInstalledVersion();
            _versionLabel!.Text = $"Installed: v{_installedVersion} | Latest: v{_remoteVersion.Version}";
            UpdatePlayButton();
        }
        else
        {
            _versionLabel!.Text = "Failed to check version";
        }
    }

    private async Task LoadPatchNotesAsync()
    {
        if (string.IsNullOrEmpty(_config?.PatchNotesUrl) || _patchNotesPanel == null) return;

        await _patchNotesPanel.LoadPatchNotesAsync(_config.PatchNotesUrl, _dataService);
    }

    private string GetInstalledVersion()
    {
        try
        {
            if (!string.IsNullOrEmpty(_config?.GameExe))
            {
                string gamePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _config.GameExe);
                if (File.Exists(gamePath))
                {
                    FileVersionInfo vInfo = FileVersionInfo.GetVersionInfo(gamePath);
                    return vInfo.FileVersion ?? "0.0.0";
                }
            }
        }
        catch
        {
        }

        return Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0";
    }

    private void UpdatePlayButton()
    {
        if (_playButton == null) return;

        bool needsUpdate = _remoteVersion != null && _remoteVersion.Version != _installedVersion;

        if (_isDownloading)
        {
            _playButton.Text = "Downloading...";
            _playButton.Enabled = false;
        }
        else if (needsUpdate)
        {
            _playButton.Text = "Update";
            _playButton.BackColor = Color.FromArgb(0, 150, 80);
            _playButton.Enabled = true;
        }
        else
        {
            _playButton.Text = "Play";
            _playButton.BackColor = Color.FromArgb(0, 120, 215);
            _playButton.Enabled = true;
        }
    }

    private async void PlayButton_Click(object? sender, EventArgs e)
    {
        if (_isDownloading) return;

        if (_remoteVersion != null && _remoteVersion.Version != _installedVersion)
        {
            await StartDownloadAsync();
        }
        else
        {
            LaunchGame();
        }
    }

    private async Task StartDownloadAsync()
    {
        if (_remoteVersion == null || _config == null || _downloadPanel == null || _downloadProgress == null || _downloadLabel == null) return;

        _isDownloading = true;
        UpdatePlayButton();
        _downloadPanel.Visible = true;
        _downloadProgress.Value = 0;
        _downloadLabel.Text = "Starting download...";

        string savePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GameUpdate.zip");

        try
        {
            await _dataService.DownloadUpdate(
                _remoteVersion.DownloadUrl ?? _config.DownloadUrl,
                savePath,
                (percent, downloaded) =>
                {
                    Invoke(new Action(() =>
                    {
                        _downloadProgress!.Value = percent;
                        _downloadLabel!.Text = $"{percent}%";
                    }));
                });

            _downloadLabel.Text = "Extracting...";
            ExtractUpdate(savePath);
            File.Delete(savePath);

            _installedVersion = _remoteVersion.Version;
            _versionLabel!.Text = $"Installed: v{_installedVersion} | Latest: v{_remoteVersion.Version}";
            _downloadPanel.Visible = false;
            _isDownloading = false;
            UpdatePlayButton();

            LaunchGame();
        }
        catch (Exception ex)
        {
            _downloadLabel.Text = $"Error: {ex.Message}";
            _isDownloading = false;
            UpdatePlayButton();
        }
    }

    private void ExtractUpdate(string zipPath)
    {
        ZipFile.ExtractToDirectory(zipPath, AppDomain.CurrentDomain.BaseDirectory, true);
    }

    private void LaunchGame()
    {
        if (_config == null) return;

        string gamePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _config.GameExe);

        if (File.Exists(gamePath))
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = gamePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to launch game: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        else
        {
            MessageBox.Show($"Game executable not found: {gamePath}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void PlayButton_Paint(object? sender, PaintEventArgs e)
    {
        Button btn = (Button)sender;
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        int radius = 8;
        using GraphicsPath path = new();
        path.AddRoundRectangle(new Rectangle(1, 1, btn.Width - 2, btn.Height - 2), radius);
        btn.Region = new Region(path);

        using SolidBrush brush = new(btn.BackColor);
        e.Graphics.FillRectangle(brush, 0, 0, btn.Width, btn.Height);
    }

    private void FadeInTimer_Tick(object? sender, EventArgs e)
    {
        if (_overlayPanel == null) return;

        Color currentColor = _overlayPanel.BackColor;
        int currentAlpha = currentColor.A;
        if (currentAlpha >= 240)
        {
            _overlayPanel!.BackColor = Color.FromArgb(60, 0, 0, 0);
            _fadeInTimer!.Stop();
            return;
        }

        int newAlpha = Math.Min(240, currentAlpha + 3);
        _overlayPanel!.BackColor = Color.FromArgb(newAlpha, 0, 0, 0);
    }

    private string? GetEmbeddedResource(string resourceName)
    {
        try
        {
            using Stream? stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(resourceName);

            if (stream == null) return null;

            using StreamReader reader = new(stream);
            return reader.ReadToEnd();
        }
        catch
        {
            return null;
        }
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);

        if (_playButton != null && _bottomPanel != null)
        {
            int centerX = (_bottomPanel.Width - 200) / 2;
            _playButton!.Location = new Point(centerX, 15);
            _versionLabel!.Location = new Point(centerX, 70);
        }

        if (_patchNotesPanel != null)
        {
            _patchNotesPanel!.Width = Width - 120;
        }

        if (_downloadPanel != null && _downloadProgress != null && _downloadLabel != null)
        {
            _downloadProgress!.Width = _downloadPanel.Width - 240;
            _downloadLabel!.Location = new Point(_downloadPanel.Width - 150, 12);
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (_backgroundPanel?.BackgroundImage != null)
        {
            _backgroundPanel.BackgroundImage.Dispose();
        }
        base.OnFormClosing(e);
    }
}

public static class GraphicsExtensions
{
    public static GraphicsPath AddRoundRectangle(this GraphicsPath path, Rectangle rectangle, int radius)
    {
        int diameter = radius * 2;
        Size size = new(diameter, diameter);
        RectangleF arc = new(rectangle.Location, size);

        path.AddArc(arc, 180, 90);
        arc.X = rectangle.Right - diameter;
        path.AddArc(arc, 270, 90);
        arc.Y = rectangle.Bottom - diameter;
        path.AddArc(arc, 0, 90);
        path.AddArc(arc, 90, 90);
        path.CloseFigure();

        return path;
    }
}
