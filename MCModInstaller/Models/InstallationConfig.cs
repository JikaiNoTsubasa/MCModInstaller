namespace MCModInstaller.Models;

public class InstallationConfig
{
    public string Version { get; set; } = "1.0";
    public string? LastInstancePath { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.Now;
}
