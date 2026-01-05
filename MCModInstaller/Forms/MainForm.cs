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

        // Validate initial path state
        ValidateAndUpdateInstallButton();
    }

    private void PathTextBox_TextChanged(object sender, EventArgs e)
    {
        ValidateAndUpdateInstallButton();
    }

    private void ValidateAndUpdateInstallButton()
    {
        var path = pathTextBox.Text;
        var (isValid, _) = _pathValidationService.ValidatePath(path);
        installButton.Enabled = isValid;
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

        // Clear previous logs
        logTextBox.Clear();
        AddLog("Démarrage de l'installation...");

        // Validate path
        AddLog("Validation du chemin d'accès...");
        var (isValid, errorMessage) = _pathValidationService.ValidatePath(path);
        if (!isValid)
        {
            AddLog($"Erreur : {errorMessage}");
            MessageBox.Show(
                errorMessage,
                Constants.ErrorTitle,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }
        AddLog("Chemin d'accès validé avec succès");

        // Disable controls
        installButton.Enabled = false;
        browseButton.Enabled = false;
        pathTextBox.Enabled = false;
        installButton.Text = Constants.InstallingButtonText;

        // Show progress bar
        progressBar.Visible = true;
        statusLabel.Visible = true;
        statusLabel.Text = Constants.StatusDownloadingModList;

        try
        {
            _cancellationTokenSource = new CancellationTokenSource();

            // Download mods list JSON
            AddLog("Téléchargement de la liste des mods (mods.json)...");
            List<string> modsList;
            try
            {
                modsList = await _downloadService.DownloadModsListAsync(_cancellationTokenSource.Token);
                AddLog($"Liste des mods téléchargée avec succès ({modsList.Count} mod(s) trouvé(s))");
            }
            catch (Exception ex)
            {
                AddLog($"Erreur lors du téléchargement de la liste : {ex.Message}");
                MessageBox.Show(
                    ex.Message,
                    Constants.ErrorTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                statusLabel.Text = Constants.StatusIdle;
                throw;
            }

            if (modsList.Count == 0)
            {
                AddLog("Erreur : La liste des mods est vide");
                MessageBox.Show(
                    "La liste des mods est vide.",
                    Constants.ErrorTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                statusLabel.Text = Constants.StatusIdle;
                throw new InvalidOperationException("La liste des mods est vide.");
            }

            // Log all mods to download
            AddLog("Mods à télécharger :");
            foreach (var mod in modsList)
            {
                AddLog($"  - {mod}");
            }

            // Download all mods
            var modsPath = _pathValidationService.GetModsPath(path);
            AddLog($"Début du téléchargement vers : {modsPath}");

            var progress = new Progress<MultiModDownloadProgress>(p =>
            {
                if (InvokeRequired)
                {
                    Invoke(() => UpdateMultiModProgress(p));
                }
                else
                {
                    UpdateMultiModProgress(p);
                }
            });

            var results = await _downloadService.DownloadMultipleModsAsync(
                modsList,
                modsPath,
                progress,
                AddLog,
                _cancellationTokenSource.Token);

            AddLog($"Téléchargement terminé, analyse des résultats... ({results.Count} mod(s) traité(s))");

            // Log results
            foreach (var result in results)
            {
                if (result.Success)
                {
                    AddLog($"✓ {result.ModFileName} - Téléchargé avec succès");
                }
                else
                {
                    AddLog($"✗ {result.ModFileName} - Échec : {result.ErrorMessage}");
                }
            }

            // Vérification locale des fichiers
            AddLog("Vérification de la présence des fichiers dans le dossier...");
            statusLabel.Text = Constants.StatusVerifyingMods;

            var missingFiles = new List<string>();
            foreach (var result in results.Where(r => r.Success))
            {
                var filePath = Path.Combine(modsPath, result.ModFileName);
                if (!File.Exists(filePath))
                {
                    AddLog($"⚠ ERREUR: {result.ModFileName} est marqué comme téléchargé mais est ABSENT du dossier!");
                    missingFiles.Add(result.ModFileName);
                    result.Success = false;
                    result.ErrorMessage = "Fichier manquant après téléchargement";
                }
                else
                {
                    var fileInfo = new FileInfo(filePath);
                    AddLog($"✓ {result.ModFileName} vérifié (Taille: {fileInfo.Length / 1024} Ko)");
                }
            }

            if (missingFiles.Any())
            {
                AddLog($"⚠ ATTENTION: {missingFiles.Count} fichier(s) manquant(s) détecté(s)!");
            }
            else
            {
                AddLog("✓ Tous les fichiers téléchargés sont présents dans le dossier");
            }

            // Save configuration
            var config = new InstallationConfig
            {
                LastInstancePath = path
            };
            _configService.SaveConfig(config);
            AddLog("Configuration sauvegardée");

            // Check for failures (including missing files)
            var failedMods = results.Where(r => !r.Success).ToList();
            var successMods = results.Where(r => r.Success).ToList();

            if (failedMods.Any())
            {
                AddLog($"Installation terminée avec {failedMods.Count} erreur(s)");

                // Séparer les erreurs de téléchargement et les fichiers manquants
                var downloadErrors = failedMods.Where(f => f.ErrorMessage != "Fichier manquant après téléchargement").ToList();
                var missingFileErrors = failedMods.Where(f => f.ErrorMessage == "Fichier manquant après téléchargement").ToList();

                var errorMsg = Constants.ErrorSomeModsFailed + "\n";

                if (missingFileErrors.Any())
                {
                    errorMsg += $"\n⚠ Fichiers manquants après téléchargement ({missingFileErrors.Count}):\n";
                    errorMsg += string.Join("\n", missingFileErrors.Select(f => $"- {f.ModFileName}"));
                }

                if (downloadErrors.Any())
                {
                    errorMsg += $"\n\nErreurs de téléchargement ({downloadErrors.Count}):\n";
                    errorMsg += string.Join("\n", downloadErrors.Select(f => $"- {f.ModFileName}: {f.ErrorMessage}"));
                }

                errorMsg += $"\n\n✓ {successMods.Count} mod(s) installé(s) avec succès";

                MessageBox.Show(
                    errorMsg,
                    Constants.ErrorTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            else
            {
                AddLog($"Installation terminée avec succès ! {results.Count} mod(s) installé(s)");
                // All mods downloaded successfully
                MessageBox.Show(
                    $"{results.Count} mod(s) ont été installés avec succès !\n\nTous les fichiers ont été vérifiés et sont présents dans le dossier.",
                    Constants.SuccessTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }

            statusLabel.Text = Constants.StatusComplete;
        }
        catch (OperationCanceledException)
        {
            AddLog("Installation annulée par l'utilisateur");
            MessageBox.Show(
                "Installation annulée.",
                Constants.ErrorTitle,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            statusLabel.Text = Constants.StatusIdle;
        }
        catch (FileNotFoundException)
        {
            AddLog($"Erreur : {Constants.ErrorFileNotFound}");
            MessageBox.Show(
                Constants.ErrorFileNotFound,
                Constants.ErrorTitle,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            statusLabel.Text = Constants.StatusIdle;
        }
        catch (UnauthorizedAccessException)
        {
            AddLog($"Erreur : {Constants.ErrorAccessDenied}");
            MessageBox.Show(
                Constants.ErrorAccessDenied,
                Constants.ErrorTitle,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            statusLabel.Text = Constants.StatusIdle;
        }
        catch (IOException ex) when (ex.Message.Contains(Constants.ErrorDiskFull))
        {
            AddLog($"Erreur : {Constants.ErrorDiskFull}");
            MessageBox.Show(
                Constants.ErrorDiskFull,
                Constants.ErrorTitle,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            statusLabel.Text = Constants.StatusIdle;
        }
        catch (Exception ex)
        {
            AddLog($"Erreur inattendue : {ex.Message}");
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

    private void UpdateMultiModProgress(MultiModDownloadProgress progress)
    {
        progressBar.Value = progress.OverallPercentComplete;
        var modProgress = progress.CurrentModProgress?.PercentComplete ?? 0;
        statusLabel.Text = $"Mod {progress.CurrentModIndex + 1}/{progress.TotalMods}: {progress.CurrentModFileName} ({modProgress}%)";
    }

    private void AddLog(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        var logMessage = $"[{timestamp}] {message}";

        if (InvokeRequired)
        {
            Invoke(() =>
            {
                logTextBox.AppendText(logMessage + Environment.NewLine);
            });
        }
        else
        {
            logTextBox.AppendText(logMessage + Environment.NewLine);
        }
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
