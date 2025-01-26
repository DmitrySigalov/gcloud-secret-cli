# gclod-secret-manager-cli

[![Build](https://github.com/dmitrysigalov/gclod-secret-manager-cli/workflows/Build/badge.svg)](https://github.com/dmitrysigalov/gclod-secret-manager-cli/actions/workflows/build.yml)
[![License](https://badgen.net/github/license/dmitrysigalov/gclod-secret-manager-cli)](https://github.com/DmitrySigalov/gclod-secret-manager-cli/blob/main/LICENSE)

A dotnet open source which provides integration with google cloud secret manager using tool

## :gift: Features:
- Best practice for the environment variable names according to secrets configured in Google Cloud secrets store
- Configuration of profiles with naming convention (profiles, google projects)
- Synchronization of environment variables with secrets according to selected profile/google project
- View current synchronization state of environment variables with secrets according to selected profile/google project
- Import/export secret values

## :sunny: .NET Runtime
This project is built with DotNet 8.0 and is mandatory to install before using.

You can find and install it [here](https://dotnet.microsoft.com/en-us/download/dotnet/8.0).

Verify your dotnet version:

![image](https://user-images.githubusercontent.com/31489258/153608978-cced639e-af42-4485-8c15-5333325b0883.png)

## :gift: Installation

The Installer publishes the code to the machine applications directory and adds it to your system's path.

Guideline:
1. Download latest release to local machine
2. Un-puck sources
3. Open terminal / cmd (as administrator)
4. Set current directory to un-packed folder with sources and installer (cd command)
5. Run installation command:

### Windows
```
dotnet run --project src/Aws.Ssm.Cli.Installer/Aws.Ssm.Cli.Installer.csproj
```

### macOS
```
sudo dotnet run --project src/Aws.Ssm.Cli.Installer/Aws.Ssm.Cli.Installer.csproj
```

Reopen terminal/cmd and run
```
gcloud-secret-manager-cli help
```
or
```
gscli help
```
If everything ran smoothly, you should see the list of supported commands.

## :tada: Usage

TODO
