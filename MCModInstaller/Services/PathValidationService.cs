using MCModInstaller.Utilities;

namespace MCModInstaller.Services;

public class PathValidationService
{
    public (bool IsValid, string? ErrorMessage) ValidatePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return (false, Constants.ErrorPathNotFound);
        }

        // Check if directory exists
        if (!Directory.Exists(path))
        {
            return (false, Constants.ErrorPathNotFound);
        }

        // Check if mods folder exists
        var modsPath = Path.Combine(path, Constants.ModsFolder);
        if (!Directory.Exists(modsPath))
        {
            return (false, Constants.ErrorModsFolderNotFound);
        }

        // Check write permissions
        if (!CheckWritePermission(modsPath))
        {
            return (false, Constants.ErrorNoWritePermission);
        }

        return (true, null);
    }

    private bool CheckWritePermission(string path)
    {
        try
        {
            var testFile = Path.Combine(path, $".test_{Guid.NewGuid()}.tmp");
            File.WriteAllText(testFile, "test");
            File.Delete(testFile);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public string GetModsPath(string instancePath)
    {
        return Path.Combine(instancePath, Constants.ModsFolder);
    }
}
