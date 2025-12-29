# MCModInstaller

Application Windows pour télécharger et installer automatiquement le mod MCPlantator dans une instance CurseForge de Minecraft.

## Fonctionnalités

- Interface graphique simple et intuitive en français
- Téléchargement automatique du mod depuis https://triedge.ovh/mcplantator-1.0.0.jar
- Validation du chemin de l'instance Minecraft
- Barre de progression avec pourcentage et vitesse de téléchargement
- Sauvegarde du chemin pour les prochaines utilisations
- Remplacement automatique si le mod existe déjà

## Utilisation

1. Lancez `MCModInstaller.exe`
2. Suivez les instructions pour trouver le chemin de votre instance CurseForge:
   - Ouvrez CurseForge
   - Clic droit sur votre modpack
   - Sélectionnez "Open folder"
   - Copiez le chemin d'accès
3. Collez le chemin dans l'application (ou utilisez le bouton "Parcourir...")
4. Cliquez sur "Installer"
5. Le mod sera téléchargé et placé dans le dossier `mods` de votre instance

## Pour les développeurs

### Prérequis

- .NET 9.0 SDK ou supérieur
- Windows 10/11

### Compilation du projet

Pour compiler le projet en mode Debug:

```bash
dotnet build MCModInstaller.sln
```

Pour lancer l'application en mode développement:

```bash
dotnet run --project MCModInstaller/MCModInstaller.csproj
```

### Génération de l'exe standalone

Pour créer un fichier .exe standalone qui peut être distribué:

```bash
dotnet publish MCModInstaller/MCModInstaller.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=None -p:DebugSymbols=false
```

L'exe sera généré dans:
```
MCModInstaller/bin/Release/net9.0-windows/win-x64/publish/MCModInstaller.exe
```

Taille approximative: ~108 MB

### Structure du projet

```
MCModInstaller/
├── Forms/                    # Interface utilisateur
│   ├── MainForm.cs          # Logique de l'interface
│   ├── MainForm.Designer.cs # Définition des contrôles
│   └── MainForm.resx        # Ressources
├── Services/                 # Services métier
│   ├── ConfigService.cs     # Gestion de la configuration
│   ├── DownloadService.cs   # Téléchargement des fichiers
│   └── PathValidationService.cs # Validation des chemins
├── Models/                   # Modèles de données
│   ├── InstallationConfig.cs
│   └── DownloadProgress.cs
├── Utilities/
│   └── Constants.cs         # Constantes de l'application
└── Program.cs               # Point d'entrée

```

## Configuration

La configuration est sauvegardée automatiquement dans:
```
%APPDATA%\MCModInstaller\mcmodinstaller.config.json
```

Les logs d'erreurs sont stockés dans:
```
%APPDATA%\MCModInstaller\logs\mcmodinstaller.log
```

## Dépannage

### "Le dossier 'mods' est introuvable"
Assurez-vous de sélectionner le dossier racine de l'instance, pas le dossier `mods` directement.

### "Impossible d'écrire dans ce dossier"
Essayez de lancer l'installateur en tant qu'administrateur.

### "Impossible de se connecter au serveur"
Vérifiez votre connexion Internet et que le serveur https://triedge.ovh est accessible.

## Version

Version actuelle: 1.0.0

## Technologies utilisées

- .NET 9.0
- Windows Forms
- C#
