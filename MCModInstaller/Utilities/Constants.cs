namespace MCModInstaller.Utilities;

public static class Constants
{
    // Download configuration
    public const string DownloadUrl = "https://triedge.ovh/mcplantator-1.0.0.jar";
    public const string ModFileName = "mcplantator-1.0.0.jar";

    // Configuration file settings
    public const string ConfigFileName = "mcmodinstaller.config.json";
    public const string AppDataFolder = "MCModInstaller";
    public const string LogsFolder = "logs";
    public const string LogFileName = "mcmodinstaller.log";

    // Minecraft folder structure
    public const string ModsFolder = "mods";

    // UI Text (French)
    public const string AppTitle = "MCModInstaller - Installation de Mod";
    public const string HeaderText = "Installation de MCPlantator Mod";

    public const string InstructionsTitle = "Comment trouver le chemin d'accès";
    public const string InstructionsText =
        "1. Ouvrir CurseForge\n" +
        "2. Clic droit sur le modpack\n" +
        "3. Sélectionner \"Open folder\"\n" +
        "4. Copier le chemin d'accès depuis l'explorateur Windows\n" +
        "5. Coller le chemin ci-dessous";

    public const string PathLabel = "Chemin de l'instance Minecraft :";
    public const string BrowseButtonText = "Parcourir...";
    public const string InstallButtonText = "Installer";
    public const string InstallingButtonText = "Installation...";
    public const string DownloadingText = "Téléchargement en cours...";

    // Status messages
    public const string StatusIdle = "Prêt à installer";
    public const string StatusValidating = "Validation du chemin...";
    public const string StatusDownloading = "Téléchargement du mod...";
    public const string StatusInstalling = "Installation du mod...";
    public const string StatusComplete = "Installation terminée !";

    // Success messages
    public const string SuccessTitle = "Installation réussie";
    public const string SuccessMessage = "Le mod a été installé avec succès !";

    // Error messages
    public const string ErrorTitle = "Erreur";
    public const string ErrorPathNotFound = "Le chemin spécifié n'existe pas.";
    public const string ErrorModsFolderNotFound = "Le dossier 'mods' est introuvable. Vérifiez que c'est bien un dossier d'instance Minecraft.";
    public const string ErrorNoWritePermission = "Impossible d'écrire dans ce dossier. Vérifiez les permissions.";
    public const string ErrorNetworkUnavailable = "Impossible de se connecter au serveur. Vérifiez votre connexion Internet.";
    public const string ErrorFileNotFound = "Le fichier mod est introuvable sur le serveur.";
    public const string ErrorTimeout = "Le téléchargement a expiré. Veuillez réessayer.";
    public const string ErrorDiskFull = "Espace disque insuffisant pour installer le mod.";
    public const string ErrorFileLocked = "Le fichier mod est utilisé par un autre programme. Fermez Minecraft et réessayez.";
    public const string ErrorAccessDenied = "Accès refusé. Essayez de lancer l'installateur en tant qu'administrateur.";
    public const string ErrorUnknown = "Une erreur inattendue s'est produite. Consultez le fichier journal pour plus de détails.";

    // FolderBrowserDialog
    public const string FolderBrowserDescription = "Sélectionnez le dossier de l'instance Minecraft CurseForge";

    // Application info
    public const string AppVersion = "1.0.0";
    public const string AppName = "MCModInstaller";
}
