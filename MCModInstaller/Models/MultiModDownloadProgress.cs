namespace MCModInstaller.Models;

public class MultiModDownloadProgress
{
    public int CurrentModIndex { get; set; }
    public int TotalMods { get; set; }
    public string CurrentModFileName { get; set; } = string.Empty;
    public DownloadProgress? CurrentModProgress { get; set; }

    public int OverallPercentComplete
    {
        get
        {
            if (TotalMods == 0) return 0;

            var completedMods = CurrentModIndex;
            var currentModPercent = CurrentModProgress?.PercentComplete ?? 0;

            var percent = (int)((completedMods * 100.0 + currentModPercent) / TotalMods);

            // Limiter à 100% maximum pour éviter les dépassements
            return Math.Min(percent, 100);
        }
    }
}
