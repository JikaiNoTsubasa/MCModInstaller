namespace MCModInstaller.Models;

public class DownloadProgress
{
    public long BytesDownloaded { get; set; }
    public long TotalBytes { get; set; }
    public int PercentComplete { get; set; }
    public double SpeedBytesPerSecond { get; set; }
    public string FormattedSpeed { get; set; } = string.Empty;

    public void Calculate()
    {
        if (TotalBytes > 0)
        {
            PercentComplete = (int)((BytesDownloaded * 100) / TotalBytes);
        }

        FormatSpeed();
    }

    private void FormatSpeed()
    {
        if (SpeedBytesPerSecond >= 1024 * 1024)
        {
            FormattedSpeed = $"{SpeedBytesPerSecond / (1024 * 1024):F2} MB/s";
        }
        else if (SpeedBytesPerSecond >= 1024)
        {
            FormattedSpeed = $"{SpeedBytesPerSecond / 1024:F2} KB/s";
        }
        else
        {
            FormattedSpeed = $"{SpeedBytesPerSecond:F0} B/s";
        }
    }
}
