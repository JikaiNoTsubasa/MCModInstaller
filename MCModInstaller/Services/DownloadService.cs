using System.Diagnostics;
using System.Text.Json;
using MCModInstaller.Models;
using MCModInstaller.Utilities;

namespace MCModInstaller.Services;

public class DownloadService
{
    private static readonly HttpClient _httpClient = new HttpClient();

    public async Task DownloadFileAsync(
        string url,
        string destinationPath,
        IProgress<DownloadProgress>? progress,
        Action<string>? logCallback = null,
        CancellationToken cancellationToken = default)
    {
        string? tempPath = null;
        try
        {
            // Create temporary file path
            tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.tmp");
            logCallback?.Invoke($"  → Fichier temp: {tempPath}");

            // Send request with ResponseHeadersRead to start downloading immediately
            logCallback?.Invoke($"  → Connexion à: {url}");
            using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logCallback?.Invoke($"  → Erreur HTTP: {response.StatusCode}");
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new FileNotFoundException(Constants.ErrorFileNotFound);
                }
                throw new HttpRequestException(Constants.ErrorNetworkUnavailable);
            }

            var totalBytes = response.Content.Headers.ContentLength ?? -1;
            logCallback?.Invoke($"  → Taille attendue: {totalBytes / 1024} Ko");
            var buffer = new byte[8192];
            long totalRead = 0;

            var stopwatch = Stopwatch.StartNew();

            using (var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken))
            using (var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
            {
                int bytesRead;
                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                    totalRead += bytesRead;

                    if (progress != null && stopwatch.ElapsedMilliseconds > 100)
                    {
                        var downloadProgress = new DownloadProgress
                        {
                            BytesDownloaded = totalRead,
                            TotalBytes = totalBytes,
                            SpeedBytesPerSecond = totalRead / (stopwatch.Elapsed.TotalSeconds + 0.001)
                        };
                        downloadProgress.Calculate();
                        progress.Report(downloadProgress);
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                }
            }

            stopwatch.Stop();
            logCallback?.Invoke($"  → Téléchargement terminé: {totalRead / 1024} Ko en {stopwatch.Elapsed.TotalSeconds:F2}s");

            // Vérifier que le fichier temp existe et a la bonne taille
            if (!File.Exists(tempPath))
            {
                throw new IOException($"Le fichier temporaire n'existe pas: {tempPath}");
            }

            var tempFileInfo = new FileInfo(tempPath);
            if (tempFileInfo.Length == 0)
            {
                throw new IOException($"Le fichier temporaire est vide: {tempPath}");
            }

            if (totalBytes > 0 && tempFileInfo.Length != totalBytes)
            {
                logCallback?.Invoke($"  → AVERTISSEMENT: Taille téléchargée ({tempFileInfo.Length}) != taille attendue ({totalBytes})");
            }

            logCallback?.Invoke($"  → Fichier temp vérifié: {tempFileInfo.Length / 1024} Ko");

            // Final progress report
            if (progress != null)
            {
                var finalProgress = new DownloadProgress
                {
                    BytesDownloaded = totalRead,
                    TotalBytes = totalBytes,
                    SpeedBytesPerSecond = totalRead / (stopwatch.Elapsed.TotalSeconds + 0.001)
                };
                finalProgress.Calculate();
                progress.Report(finalProgress);
            }

            // Delete existing file if it exists
            if (File.Exists(destinationPath))
            {
                logCallback?.Invoke($"  → Suppression de l'ancien fichier: {Path.GetFileName(destinationPath)}");
                try
                {
                    File.Delete(destinationPath);
                }
                catch (Exception ex)
                {
                    logCallback?.Invoke($"  → ERREUR lors de la suppression: {ex.Message}");
                    throw new IOException($"Impossible de supprimer l'ancien fichier: {ex.Message}");
                }
            }

            // Move temp file to destination (atomic operation)
            logCallback?.Invoke($"  → Déplacement vers: {destinationPath}");
            try
            {
                File.Move(tempPath, destinationPath);
            }
            catch (Exception ex)
            {
                logCallback?.Invoke($"  → ERREUR lors du déplacement: {ex.Message}");
                throw new IOException($"Impossible de déplacer le fichier: {ex.Message}");
            }

            // Vérifier que le fichier final existe
            if (!File.Exists(destinationPath))
            {
                throw new IOException($"Le fichier de destination n'existe pas après le déplacement: {destinationPath}");
            }

            var finalFileInfo = new FileInfo(destinationPath);
            logCallback?.Invoke($"  → Fichier final vérifié: {finalFileInfo.Length / 1024} Ko");

            // Nettoyer le fichier temp s'il existe encore
            if (File.Exists(tempPath))
            {
                try
                {
                    File.Delete(tempPath);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }
        catch (OperationCanceledException)
        {
            logCallback?.Invoke($"  → Téléchargement annulé");
            throw;
        }
        catch (HttpRequestException ex)
        {
            logCallback?.Invoke($"  → Erreur réseau: {ex.Message}");
            throw new Exception(Constants.ErrorNetworkUnavailable);
        }
        catch (IOException ex) when (ex.Message.Contains("space"))
        {
            logCallback?.Invoke($"  → Erreur espace disque: {ex.Message}");
            throw new IOException(Constants.ErrorDiskFull);
        }
        catch (UnauthorizedAccessException ex)
        {
            logCallback?.Invoke($"  → Erreur accès refusé: {ex.Message}");
            throw new UnauthorizedAccessException(Constants.ErrorAccessDenied);
        }
        catch (Exception ex) when (ex is not FileNotFoundException)
        {
            logCallback?.Invoke($"  → Erreur inattendue: {ex.Message}");
            throw new Exception($"{Constants.ErrorUnknown}\n\nDétails: {ex.Message}");
        }
        finally
        {
            // Cleanup: supprimer le fichier temp s'il existe encore
            if (tempPath != null && File.Exists(tempPath))
            {
                try
                {
                    File.Delete(tempPath);
                    logCallback?.Invoke($"  → Fichier temp nettoyé");
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }
    }

    public async Task<List<string>> DownloadModsListAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(Constants.ModsJsonUrl, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(Constants.ErrorJsonDownload);
            }

            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var modsList = JsonSerializer.Deserialize<List<string>>(jsonContent);

            if (modsList == null)
            {
                throw new JsonException(Constants.ErrorJsonParse);
            }

            return modsList;
        }
        catch (HttpRequestException)
        {
            throw new Exception(Constants.ErrorJsonDownload);
        }
        catch (JsonException)
        {
            throw new Exception(Constants.ErrorJsonParse);
        }
    }

    public async Task<List<ModDownloadResult>> DownloadMultipleModsAsync(
        List<string> modFileNames,
        string modsPath,
        IProgress<MultiModDownloadProgress>? progress,
        Action<string>? logCallback = null,
        CancellationToken cancellationToken = default)
    {
        var results = new List<ModDownloadResult>();

        for (int i = 0; i < modFileNames.Count; i++)
        {
            var modFileName = modFileNames[i];
            var result = new ModDownloadResult
            {
                ModFileName = modFileName,
                Success = false
            };

            // Log début du téléchargement
            logCallback?.Invoke($"Téléchargement de {modFileName} ({i + 1}/{modFileNames.Count})...");

            try
            {
                var modUrl = Constants.ModsBaseUrl + modFileName;
                var destinationPath = Path.Combine(modsPath, modFileName);

                var modProgress = new Progress<DownloadProgress>(p =>
                {
                    progress?.Report(new MultiModDownloadProgress
                    {
                        CurrentModIndex = i,
                        TotalMods = modFileNames.Count,
                        CurrentModFileName = modFileName,
                        CurrentModProgress = p
                    });
                });

                await DownloadFileAsync(modUrl, destinationPath, modProgress, logCallback, cancellationToken);
                result.Success = true;

                // Petit délai entre chaque téléchargement pour éviter de surcharger le serveur
                if (i < modFileNames.Count - 1)
                {
                    logCallback?.Invoke($"  → Pause de 500ms avant le prochain téléchargement...");
                    await Task.Delay(500, cancellationToken);
                }
            }
            catch (FileNotFoundException)
            {
                result.ErrorMessage = $"Le fichier '{modFileName}' est introuvable sur le serveur.";
            }
            catch (UnauthorizedAccessException)
            {
                result.ErrorMessage = Constants.ErrorAccessDenied;
            }
            catch (IOException ex) when (ex.Message.Contains("space"))
            {
                result.ErrorMessage = Constants.ErrorDiskFull;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = $"Erreur: {ex.Message}";
            }

            results.Add(result);

            // Toujours rapporter la progression après chaque mod (succès ou échec)
            progress?.Report(new MultiModDownloadProgress
            {
                CurrentModIndex = i + 1,
                TotalMods = modFileNames.Count,
                CurrentModFileName = modFileName,
                CurrentModProgress = null
            });
        }

        return results;
    }
}
