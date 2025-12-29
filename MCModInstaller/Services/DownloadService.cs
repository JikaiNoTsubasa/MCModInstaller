using System.Diagnostics;
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
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Create temporary file path
            var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.tmp");

            // Send request with ResponseHeadersRead to start downloading immediately
            using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new FileNotFoundException(Constants.ErrorFileNotFound);
                }
                throw new HttpRequestException(Constants.ErrorNetworkUnavailable);
            }

            var totalBytes = response.Content.Headers.ContentLength ?? -1;
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
                File.Delete(destinationPath);
            }

            // Move temp file to destination (atomic operation)
            File.Move(tempPath, destinationPath);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (HttpRequestException)
        {
            throw new Exception(Constants.ErrorNetworkUnavailable);
        }
        catch (IOException ex) when (ex.Message.Contains("space"))
        {
            throw new IOException(Constants.ErrorDiskFull);
        }
        catch (UnauthorizedAccessException)
        {
            throw new UnauthorizedAccessException(Constants.ErrorAccessDenied);
        }
        catch (Exception ex) when (ex is not FileNotFoundException)
        {
            throw new Exception($"{Constants.ErrorUnknown}\n\nDétails: {ex.Message}");
        }
    }
}
