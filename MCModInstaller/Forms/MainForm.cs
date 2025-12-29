using MCModInstaller.Models;
using MCModInstaller.Services;
using MCModInstaller.Utilities;

namespace MCModInstaller.Forms;

public partial class MainForm : Form
{
    private readonly ConfigService _configService;
    private readonly PathValidationService _pathValidationService;
    private readonly DownloadService _downloadService;
    private CancellationTokenSource? _cancellationTokenSource;

    public MainForm()
    {
        InitializeComponent();
        _configService = new ConfigService();
        _pathValidationService = new PathValidationService();
        _downloadService = new DownloadService();
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
        // Load saved configuration
        var config = _configService.LoadConfig();
        if (!string.IsNullOrWhiteSpace(config.LastInstancePath))
        {
            pathTextBox.Text = config.LastInstancePath;
        }
    }

    private void BrowseButton_Click(object sender, EventArgs e)
    {
        using var folderBrowserDialog = new FolderBrowserDialog
        {
            Description = Constants.FolderBrowserDescription,
            ShowNewFolderButton = false
        };

        if (!string.IsNullOrWhiteSpace(pathTextBox.Text) && Directory.Exists(pathTextBox.Text))
        {
            folderBrowserDialog.SelectedPath = pathTextBox.Text;
        }

        if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
        {
            pathTextBox.Text = folderBrowserDialog.SelectedPath;
        }
    }

    private async void InstallButton_Click(object sender, EventArgs e)
    {
        var path = pathTextBox.Text;

        // Validate path
        var (isValid, errorMessage) = _pathValidationService.ValidatePath(path);
        if (!isValid)
        {
            MessageBox.Show(
                errorMessage,
                Constants.ErrorTitle,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        // Disable controls
        installButton.Enabled = false;
        browseButton.Enabled = false;
        pathTextBox.Enabled = false;
        installButton.Text = Constants.InstallingButtonText;

        // Show progress bar
        progressBar.Visible = true;
        statusLabel.Visible = true;
        statusLabel.Text = Constants.StatusDownloading;

        try
        {
            _cancellationTokenSource = new CancellationTokenSource();

            var modsPath = _pathValidationService.GetModsPath(path);
            var destinationFile = Path.Combine(modsPath, Constants.ModFileName);

            var progress = new Progress<DownloadProgress>(p =>
            {
                if (InvokeRequired)
                {
                    Invoke(() => UpdateProgress(p));
                }
                else
                {
                    UpdateProgress(p);
                }
            });

            await _downloadService.DownloadFileAsync(
                Constants.DownloadUrl,
                destinationFile,
                progress,
                _cancellationTokenSource.Token);

            // Save configuration
            var config = new InstallationConfig
            {
                LastInstancePath = path
            };
            _configService.SaveConfig(config);

            // Show success message
            MessageBox.Show(
                Constants.SuccessMessage,
                Constants.SuccessTitle,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            statusLabel.Text = Constants.StatusComplete;
        }
        catch (OperationCanceledException)
        {
            MessageBox.Show(
                "Installation annulée.",
                Constants.ErrorTitle,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            statusLabel.Text = Constants.StatusIdle;
        }
        catch (FileNotFoundException)
        {
            MessageBox.Show(
                Constants.ErrorFileNotFound,
                Constants.ErrorTitle,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            statusLabel.Text = Constants.StatusIdle;
        }
        catch (UnauthorizedAccessException)
        {
            MessageBox.Show(
                Constants.ErrorAccessDenied,
                Constants.ErrorTitle,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            statusLabel.Text = Constants.StatusIdle;
        }
        catch (IOException ex) when (ex.Message.Contains(Constants.ErrorDiskFull))
        {
            MessageBox.Show(
                Constants.ErrorDiskFull,
                Constants.ErrorTitle,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            statusLabel.Text = Constants.StatusIdle;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"{Constants.ErrorUnknown}\n\nDétails: {ex.Message}",
                Constants.ErrorTitle,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            statusLabel.Text = Constants.StatusIdle;
        }
        finally
        {
            // Re-enable controls
            installButton.Enabled = true;
            browseButton.Enabled = true;
            pathTextBox.Enabled = true;
            installButton.Text = Constants.InstallButtonText;

            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }

    private void UpdateProgress(DownloadProgress progress)
    {
        progressBar.Value = progress.PercentComplete;
        statusLabel.Text = $"{progress.PercentComplete}% - {progress.FormattedSpeed}";
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
        {
            var result = MessageBox.Show(
                "Un téléchargement est en cours. Voulez-vous vraiment quitter ?",
                "Confirmation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.No)
            {
                e.Cancel = true;
                return;
            }

            _cancellationTokenSource.Cancel();
        }

        base.OnFormClosing(e);
    }
}
