using System.Net;
using System.Windows.Forms;
using GameLauncher.Models;
using GameLauncher.Services;

namespace GameLauncher.Controls;

public class PatchNotesPanel : Panel
{
    private List<PatchNoteEntry> _entries = new();
    private FlowLayoutPanel _flowPanel;
    private Panel _scrollPanel;

    public event EventHandler<List<PatchNoteEntry>?>? PatchNotesLoaded;

    public PatchNotesPanel()
    {
        Dock = DockStyle.Right;
        Width = 380;
        BackColor = Color.FromArgb(0, 0, 0, 0);

        _scrollPanel = new Panel();
        _scrollPanel.Dock = DockStyle.Fill;
        _scrollPanel.AutoScroll = true;
        _scrollPanel.BackColor = Color.Transparent;

        _flowPanel = new FlowLayoutPanel();
        _flowPanel.Dock = DockStyle.Fill;
        _flowPanel.FlowDirection = FlowDirection.TopDown;
        _flowPanel.WrapContents = true;
        _flowPanel.Padding = new Padding(20, 30, 20, 20);
        _flowPanel.BackColor = Color.Transparent;
        _flowPanel.AutoScroll = false;

        _scrollPanel.Controls.Add(_flowPanel);
        Controls.Add(_scrollPanel);
    }

    public async Task LoadPatchNotesAsync(string url, DataService dataService)
    {
        _flowPanel.Controls.Clear();
        await Task.Delay(200);

        List<PatchNoteEntry>? entries = await dataService.LoadPatchNotes(url);

        if (entries != null && entries.Count > 0)
        {
            _entries = entries;
            _entries.Reverse();

            Invoke(new Action(() =>
            {
                BuildPatchNotesUI();
                PatchNotesLoaded?.Invoke(this, _entries);
            }));
        }
    }

    private void BuildPatchNotesUI()
    {
        _flowPanel.Controls.Clear();

        foreach (PatchNoteEntry entry in _entries)
        {
            Panel card = CreatePatchNoteCard(entry);
            _flowPanel.Controls.Add(card);
        }
    }

    private Panel CreatePatchNoteCard(PatchNoteEntry entry)
    {
        Panel card = new();
        card.Width = 340;
        card.Height = 220;
        card.BackColor = Color.FromArgb(40, 40, 40, 40);
        card.Margin = new Padding(0, 0, 0, 16);
        card.Paint += (s, e) =>
        {
            using Pen pen = new(Color.FromArgb(80, 80, 80), 1);
            e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
        };

        PictureBox pictureBox = new();
        pictureBox.Location = new Point(10, 10);
        pictureBox.Size = new Size(320, 130);
        pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
        pictureBox.BackColor = Color.FromArgb(30, 30, 30);
        pictureBox.BorderStyle = BorderStyle.None;

        if (!string.IsNullOrEmpty(entry.ImageUrl))
        {
            LoadImageAsync(pictureBox, entry.ImageUrl);
        }
        else
        {
            pictureBox.Image = GeneratePlaceholderImage();
        }

        Label titleLabel = new();
        titleLabel.Location = new Point(10, 150);
        titleLabel.Size = new Size(320, 25);
        titleLabel.Text = entry.Title;
        titleLabel.Font = new Font("Segoe UI", 11, FontStyle.Bold);
        titleLabel.ForeColor = Color.White;
        titleLabel.AutoSize = true;

        Label descLabel = new();
        descLabel.Location = new Point(10, 178);
        descLabel.Size = new Size(320, 35);
        descLabel.Text = entry.Description;
        descLabel.Font = new Font("Segoe UI", 9);
        descLabel.ForeColor = Color.FromArgb(180, 180, 180);
        descLabel.AutoSize = true;
        descLabel.MaximumSize = new Size(320, 0);

        Label dateLabel = new();
        dateLabel.Location = new Point(10, 200);
        dateLabel.Size = new Size(320, 15);
        dateLabel.Text = entry.Date;
        dateLabel.Font = new Font("Segoe UI", 8);
        dateLabel.ForeColor = Color.FromArgb(120, 120, 120);
        dateLabel.AutoSize = true;

        card.Controls.Add(pictureBox);
        card.Controls.Add(titleLabel);
        card.Controls.Add(descLabel);
        card.Controls.Add(dateLabel);

        return card;
    }

    private async void LoadImageAsync(PictureBox pictureBox, string url)
    {
        try
        {
            byte[]? data = await new HttpClient().GetByteArrayAsync(url);
            if (data != null && data.Length > 0)
            {
                using MemoryStream ms = new(data);
                Image img = Image.FromStream(ms);
                pictureBox.Image = img;
            }
        }
        catch
        {
            pictureBox.Image = GeneratePlaceholderImage();
        }
    }

    private Image GeneratePlaceholderImage()
    {
        Bitmap bmp = new(320, 130);
        using Graphics g = Graphics.FromImage(bmp);
        g.Clear(Color.FromArgb(50, 50, 50));

        using SolidBrush brush = new(Color.FromArgb(100, 100, 100));
        using Font font = new("Segoe UI", 14);

        string text = "No Image";
        SizeF textSize = g.MeasureString(text, font);
        PointF center = new(
            (bmp.Width - textSize.Width) / 2,
            (bmp.Height - textSize.Height) / 2);

        g.DrawString(text, font, brush, center);

        return bmp;
    }

    public void Clear()
    {
        _entries.Clear();
        _flowPanel.Controls.Clear();
    }
}
