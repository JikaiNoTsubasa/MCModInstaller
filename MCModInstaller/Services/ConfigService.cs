using System.Text.Json;
using MCModInstaller.Models;
using MCModInstaller.Utilities;

namespace MCModInstaller.Services;

public class ConfigService
{
    private readonly string _configDirectory;
    private readonly string _configFilePath;

    public ConfigService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _configDirectory = Path.Combine(appData, Constants.AppDataFolder);
        _configFilePath = Path.Combine(_configDirectory, Constants.ConfigFileName);
    }

    public InstallationConfig LoadConfig()
    {
        try
        {
            EnsureConfigDirectoryExists();

            if (!File.Exists(_configFilePath))
            {
                return new InstallationConfig();
            }

            var json = File.ReadAllText(_configFilePath);
            var config = JsonSerializer.Deserialize<InstallationConfig>(json);
            return config ?? new InstallationConfig();
        }
        catch (Exception ex)
        {
            LogError($"Error loading config: {ex.Message}");
            return new InstallationConfig();
        }
    }

    public void SaveConfig(InstallationConfig config)
    {
        try
        {
            EnsureConfigDirectoryExists();

            config.LastUpdated = DateTime.Now;

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var json = JsonSerializer.Serialize(config, options);
            File.WriteAllText(_configFilePath, json);
        }
        catch (Exception ex)
        {
            LogError($"Error saving config: {ex.Message}");
        }
    }

    private void EnsureConfigDirectoryExists()
    {
        if (!Directory.Exists(_configDirectory))
        {
            Directory.CreateDirectory(_configDirectory);
        }
    }

    private void LogError(string message)
    {
        try
        {
            var logsDirectory = Path.Combine(_configDirectory, Constants.LogsFolder);
            if (!Directory.Exists(logsDirectory))
            {
                Directory.CreateDirectory(logsDirectory);
            }

            var logFilePath = Path.Combine(logsDirectory, Constants.LogFileName);
            var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [ERROR] {message}{Environment.NewLine}";
            File.AppendAllText(logFilePath, logMessage);
        }
        catch
        {
            // Silently fail if logging fails
        }
    }
}
