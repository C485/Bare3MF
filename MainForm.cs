using System.ComponentModel;
using System.Diagnostics;
using Bare3MF.Models;
using Bare3MF.Services;

namespace Bare3MF;

public class MainForm : Form
{
    private const string AttributionUrl = "https://www.flaticon.com/free-icons/3mf";

    private static readonly Color BackgroundColor = Color.FromArgb(30, 30, 46);
    private static readonly Color ForegroundColor = Color.FromArgb(205, 214, 244);
    private static readonly Color AccentColor = Color.FromArgb(137, 180, 250);
    private static readonly Color AccentTextColor = Color.FromArgb(17, 17, 27);
    private static readonly Color SecondaryButtonColor = Color.FromArgb(69, 71, 90);
    private static readonly Color DropPanelColor = Color.FromArgb(43, 44, 61);
    private static readonly Color DropPanelActiveColor = Color.FromArgb(56, 57, 80);
    private static readonly Color LogBackgroundColor = Color.FromArgb(24, 24, 37);
    private static readonly Color HintColor = Color.FromArgb(166, 173, 200);

    private readonly Button _openFilesButton;
    private readonly Button _openFolderButton;
    private readonly Panel _dropPanel;
    private readonly Label _dropLabel;
    private readonly Label _hintLabel;
    private readonly Label _versionLabel;
    private readonly LinkLabel _updateLink;
    private readonly ListBox _logBox;
    private readonly LinkLabel _attributionLink;
    private readonly ProgressBar _progressBar;
    private UpdateCheckResult? _updateCheck;

    public MainForm()
    {
        var version = AppVersion.Display;
        var applicationIcon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath);

        if (applicationIcon is not null)
        {
            Icon = applicationIcon;
        }

        Text = $"Bare3MF v{version}";
        Size = new(720, 480);
        MinimumSize = new(560, 480);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new("Segoe UI", 10);
        BackColor = BackgroundColor;
        ForeColor = ForegroundColor;

        var titleLabel = new Label
        {
            Text = "Bare3MF",
            Font = new("Segoe UI", 18, FontStyle.Bold),
            ForeColor = AccentColor,
            AutoSize = true,
            Location = new(24, 20),
        };

        var subtitleLabel = new Label
        {
            Text = "Remove bundled slicer profiles. Keep model and color data.",
            AutoSize = true,
            Location = new(24, 56),
        };

        _updateLink = new LinkLabel
        {
            Text = "Check for updates",
            Font = new("Segoe UI", 9),
            LinkColor = AccentColor,
            ActiveLinkColor = AccentColor,
            VisitedLinkColor = AccentColor,
            LinkBehavior = LinkBehavior.HoverUnderline,
            AutoSize = true,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
        };
        _updateLink.LinkArea = new(0, _updateLink.Text.Length);
        _updateLink.LinkClicked += OnUpdateLinkClicked;

        _openFilesButton = CreateButton(
            "Open 3MF file(s)",
            new(24, 96),
            new(160, 40),
            AccentColor,
            AccentTextColor);
        _openFilesButton.Click += OnOpenFilesClick;

        _openFolderButton = CreateButton(
            "Open folder",
            new(196, 96),
            new(140, 40),
            SecondaryButtonColor,
            Color.White);
        _openFolderButton.Click += OnOpenFolderClick;

        _dropPanel = new Panel
        {
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = DropPanelColor,
            Size = new(0, 140),
            Location = new(24, 152),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            AllowDrop = true,
        };
        _dropPanel.DragEnter += OnDragEnter;
        _dropPanel.DragDrop += OnDragDrop;
        _dropPanel.Paint += OnDropPanelPaint;
        _dropPanel.Resize += OnDropPanelResize;

        _dropLabel = new Label
        {
            Text = "Drop 3MF files or a folder here",
            Font = new("Segoe UI", 12, FontStyle.Bold),
            ForeColor = AccentColor,
            AutoSize = true,
        };
        _dropPanel.Controls.Add(_dropLabel);

        _hintLabel = new Label
        {
            Text = "You can also drop multiple files",
            ForeColor = HintColor,
            AutoSize = true,
        };
        _dropPanel.Controls.Add(_hintLabel);

        CenterDropLabels();

        _progressBar = new ProgressBar
        {
            Style = ProgressBarStyle.Continuous,
            Minimum = 0,
            Maximum = 100,
            Value = 0,
            Size = new(0, 12),
            Location = new(24, 306),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            Visible = false,
        };

        _logBox = new ListBox
        {
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = LogBackgroundColor,
            ForeColor = ForegroundColor,
            IntegralHeight = false,
            ItemHeight = 20,
            Location = new(24, 330),
            Size = new(0, 0),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
        };

        _versionLabel = new Label
        {
            Text = $"v{version} · Created by Kamil Skoracki",
            Font = new("Segoe UI", 8.25F),
            ForeColor = HintColor,
            AutoSize = true,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
        };

        _attributionLink = new LinkLabel
        {
            Text = "3mf icons created by designhub - Flaticon",
            Font = new("Segoe UI", 8.25F),
            LinkColor = HintColor,
            ActiveLinkColor = AccentColor,
            VisitedLinkColor = HintColor,
            LinkBehavior = LinkBehavior.HoverUnderline,
            AutoSize = true,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
        };
        _attributionLink.LinkArea = new LinkArea(0, _attributionLink.Text.Length);
        _attributionLink.LinkClicked += OnAttributionLinkClicked;

        Controls.AddRange(
        [
            titleLabel,
            subtitleLabel,
            _updateLink,
            _openFilesButton,
            _openFolderButton,
            _dropPanel,
            _progressBar,
            _logBox,
            _versionLabel,
            _attributionLink,
        ]);

        Shown += OnFormShown;
        Resize += OnFormResize;
        OnFormResize(this, EventArgs.Empty);
    }

    private static Button CreateButton(
        string text,
        Point location,
        Size size,
        Color backColor,
        Color foreColor,
        bool bold = true)
    {
        var button = new Button
        {
            Text = text,
            FlatStyle = FlatStyle.Flat,
            BackColor = backColor,
            ForeColor = foreColor,
            Font = new("Segoe UI", 10, bold ? FontStyle.Bold : FontStyle.Regular),
            Size = size,
            Location = location,
            Cursor = Cursors.Hand,
        };

        button.FlatAppearance.BorderSize = 0;
        return button;
    }

    private void OnFormResize(object? sender, EventArgs e)
    {
        var width = ClientSize.Width - 48;
        _dropPanel.Width = width;
        _progressBar.Width = width;
        _logBox.Width = width;
        _logBox.Height = ClientSize.Height - _logBox.Top - 44;
        _updateLink.Location = new(ClientSize.Width - _updateLink.Width - 24, 28);
        _versionLabel.Location = new(24, ClientSize.Height - _versionLabel.Height - 6);
        _attributionLink.Location = new(
            _versionLabel.Right + 12,
            ClientSize.Height - _attributionLink.Height - 6);
        CenterDropLabels();
        _dropPanel.Invalidate();
    }

    private void CenterDropLabels()
    {
        _dropLabel.Location = new(
            (_dropPanel.ClientSize.Width - _dropLabel.Width) / 2,
            (_dropPanel.ClientSize.Height - _dropLabel.Height - _hintLabel.Height - 8) / 2);
        _hintLabel.Location = new(
            (_dropPanel.ClientSize.Width - _hintLabel.Width) / 2,
            _dropLabel.Bottom + 8);
    }

    private void OnDropPanelResize(object? sender, EventArgs e)
    {
        CenterDropLabels();
        _dropPanel.Invalidate();
    }

    private void OnDropPanelPaint(object? sender, PaintEventArgs e)
    {
        var rect = _dropPanel.ClientRectangle;
        e.Graphics.SetClip(rect);
        e.Graphics.Clear(_dropPanel.BackColor);

        using var pen = new Pen(AccentColor, 2)
        {
            DashStyle = System.Drawing.Drawing2D.DashStyle.Dash,
        };

        var dashedRect = new Rectangle(6, 6, rect.Width - 13, rect.Height - 13);
        e.Graphics.DrawRectangle(pen, dashedRect);
    }

    private void OnDragEnter(object? sender, DragEventArgs e)
    {
        if (e.Data?.GetDataPresent(DataFormats.FileDrop) is true)
        {
            e.Effect = DragDropEffects.Copy;
            _dropPanel.BackColor = DropPanelActiveColor;
        }
    }

    private async void OnDragDrop(object? sender, DragEventArgs e)
    {
        _dropPanel.BackColor = DropPanelColor;

        if (e.Data?.GetData(DataFormats.FileDrop) is not string[] { Length: > 0 } paths)
        {
            return;
        }

        await ProcessFilesAsync(ExpandPaths(paths));
    }

    private async void OnOpenFilesClick(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Multiselect = true,
            Filter = "3MF files (*.3mf)|*.3mf|All files (*.*)|*.*",
            Title = "Select 3MF files to sanitize",
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            await ProcessFilesAsync(dialog.FileNames);
        }
    }

    private async void OnOpenFolderClick(object? sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Select a folder containing 3MF files",
            ShowNewFolderButton = false,
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            await ProcessFilesAsync(
                Directory.GetFiles(dialog.SelectedPath, "*.3mf", SearchOption.AllDirectories));
        }
    }

    private static IReadOnlyList<string> ExpandPaths(IEnumerable<string> paths)
    {
        List<string> result = [];

        foreach (var path in paths)
        {
            if (File.Exists(path) && path.EndsWith(".3mf", StringComparison.OrdinalIgnoreCase))
            {
                result.Add(path);
            }
            else if (Directory.Exists(path))
            {
                result.AddRange(Directory.GetFiles(path, "*.3mf", SearchOption.AllDirectories));
            }
        }

        return result;
    }

    private async Task ProcessFilesAsync(IReadOnlyList<string> files)
    {
        if (files.Count is 0)
        {
            MessageBox.Show(
                "No 3MF files found.",
                "Bare3MF",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        SetProcessingState(true);
        _progressBar.Value = 0;
        _logBox.Items.Clear();

        var defaultDir = Path.GetDirectoryName(files[0]) is { Length: > 0 } directory
            ? directory
            : Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        bool? overwriteAll = null;
        string? saveFolder = null;
        var processed = 0;
        var errors = 0;

        try
        {
            for (var i = 0; i < files.Count; i++)
            {
                var file = files[i];
                var name = Path.GetFileName(file);

                try
                {
                    _logBox.Items.Add($"Processing: {name}");

                    var destinationPath = ResolveDestinationPath(
                        file,
                        defaultDir,
                        ref overwriteAll,
                        ref saveFolder);

                    if (destinationPath is null)
                    {
                        _logBox.Items.Add("Cancelled by user.");
                        break;
                    }

                    var removed = await Task.Run(
                        () => ConfigSanitizer.SanitizeFile(file, destinationPath));
                    processed++;

                    var destinationName = Path.GetFileName(destinationPath);
                    _logBox.Items.Add(removed > 0
                        ? $"  ✓ {removed} config(s) removed -> {destinationName}"
                        : $"  • No configs to remove -> {destinationName}");
                }
                catch (Exception ex)
                {
                    errors++;
                    _logBox.Items.Add($"  ✗ Error on {name}: {ex.Message}");
                }

                var progress = (int)((i + 1) / (double)files.Count * 100);
                _progressBar.Value = progress;
            }
        }
        finally
        {
            SetProcessingState(false);
        }

        if (processed > 0)
        {
            MessageBox.Show(
                $"Processed {processed} file(s).{Environment.NewLine}Errors: {errors}",
                "Bare3MF",
                MessageBoxButtons.OK,
                errors > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
        }
    }

    private async void OnFormShown(object? sender, EventArgs e)
    {
        await CheckForUpdatesAsync(showUpToDateMessage: false);
    }

    private async void OnUpdateLinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
    {
        if (_updateCheck is { IsUpdateAvailable: true } update)
        {
            OpenUrl(update.ReleaseUrl);
            return;
        }

        await CheckForUpdatesAsync(showUpToDateMessage: true);
    }

    private async Task CheckForUpdatesAsync(bool showUpToDateMessage)
    {
        SetUpdateLink("Checking for updates…", enabled: false);

        var update = await UpdateService.CheckAsync(AppVersion.Current);

        if (IsDisposed || Disposing)
            return;

        _updateCheck = update;

        if (update is null)
        {
            SetUpdateLink("Check for updates");

            if (showUpToDateMessage)
            {
                MessageBox.Show(
                    "Could not retrieve the latest GitHub release. The repository may not have a published release yet, or the network is unavailable.",
                    "Bare3MF",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

            return;
        }

        if (update.IsUpdateAvailable)
        {
            SetUpdateLink($"Update available: {update.LatestTag}");
            return;
        }

        SetUpdateLink("Check for updates");

        if (showUpToDateMessage)
        {
            MessageBox.Show(
                $"You are running the latest version ({update.LatestTag}).",
                "Bare3MF",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }

    private void SetUpdateLink(string text, bool enabled = true)
    {
        _updateLink.Enabled = enabled;
        _updateLink.Text = text;
        _updateLink.LinkArea = new(0, text.Length);
        _updateLink.Location = new(ClientSize.Width - _updateLink.Width - 24, 28);
    }

    private void OnAttributionLinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
    {
        OpenUrl(AttributionUrl);
    }

    private static void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException)
        {
            MessageBox.Show(
                $"Could not open the link: {ex.Message}",
                "Bare3MF",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }

    private void SetProcessingState(bool isProcessing)
    {
        _openFilesButton.Enabled = !isProcessing;
        _openFolderButton.Enabled = !isProcessing;
        _progressBar.Visible = isProcessing;
    }

    private string? ResolveDestinationPath(
        string sourcePath,
        string defaultDir,
        ref bool? overwriteAll,
        ref string? saveFolder)
    {
        if (overwriteAll == true)
        {
            return sourcePath;
        }

        if (!string.IsNullOrEmpty(saveFolder))
        {
            return Path.Combine(saveFolder, Path.GetFileName(sourcePath));
        }

        var result = PromptDestination(sourcePath, defaultDir, ref overwriteAll, ref saveFolder);

        return result switch
        {
            DialogResult.Yes => sourcePath,
            DialogResult.No => Path.Combine(saveFolder ?? defaultDir, Path.GetFileName(sourcePath)),
            _ => null,
        };
    }

    private DialogResult PromptDestination(
        string sourcePath,
        string defaultDir,
        ref bool? overwriteAll,
        ref string? saveFolder)
    {
        var name = Path.GetFileName(sourcePath);

        using var form = new Form
        {
            Text = "Save sanitized file",
            Size = new(520, 240),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = BackgroundColor,
            ForeColor = ForegroundColor,
            Font = Font,
        };

        var message = new Label
        {
            Text = $"Where do you want to save the sanitized version of:\n{name}",
            Location = new(24, 24),
            Size = new(460, 50),
        };

        var overrideButton = CreateButton(
            "Overwrite existing",
            new(24, 120),
            new(140, 36),
            AccentColor,
            AccentTextColor,
            bold: false);
        overrideButton.DialogResult = DialogResult.Yes;

        var saveButton = CreateButton(
            "Save to folder…",
            new(176, 120),
            new(140, 36),
            SecondaryButtonColor,
            Color.White,
            bold: false);
        saveButton.DialogResult = DialogResult.No;

        var cancelButton = CreateButton(
            "Cancel",
            new(340, 120),
            new(100, 36),
            SecondaryButtonColor,
            Color.White,
            bold: false);
        cancelButton.DialogResult = DialogResult.Cancel;

        var applyAllCheck = new CheckBox
        {
            Text = "Apply this choice to all remaining files",
            Location = new(24, 170),
            AutoSize = true,
            ForeColor = ForegroundColor,
        };

        form.Controls.AddRange(
        [
            message,
            overrideButton,
            saveButton,
            cancelButton,
            applyAllCheck,
        ]);
        form.AcceptButton = overrideButton;
        form.CancelButton = cancelButton;

        var result = form.ShowDialog(this);

        if (result == DialogResult.No)
        {
            using var folderDialog = new FolderBrowserDialog
            {
                Description = "Choose a folder to save sanitized 3MF files",
                SelectedPath = defaultDir,
            };

            if (folderDialog.ShowDialog(form) == DialogResult.OK)
            {
                saveFolder = folderDialog.SelectedPath;
            }
            else
            {
                return DialogResult.Cancel;
            }
        }

        if (applyAllCheck.Checked)
        {
            overwriteAll = result == DialogResult.Yes;

            if (result != DialogResult.Yes)
            {
                saveFolder ??= defaultDir;
            }
        }

        return result;
    }
}
