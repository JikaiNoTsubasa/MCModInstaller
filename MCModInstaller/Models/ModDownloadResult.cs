namespace MCModInstaller.Models;

public class ModDownloadResult
{
    public string ModFileName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}
