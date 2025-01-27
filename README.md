# gclod-secret-cli

[![Build](https://github.com/dmitrysigalov/gclod-secret-cli/workflows/Build/badge.svg)](https://github.com/dmitrysigalov/gclod-secret-cli/actions/workflows/build.yml)
[![License](https://badgen.net/github/license/dmitrysigalov/gclod-secret-cli)](https://github.com/DmitrySigalov/gclod-secret-cli/blob/main/LICENSE)

A dotnet open source which provides integration with google cloud secret manager

## :gift: Features:
- Best practice for the environment variable names according to secrets configured in Google Cloud secrets store
- Configuration of profiles with naming convention (profiles, google projects)
- Synchronization of environment variables with secrets according to selected profile/google project
- View current synchronization state of environment variables with secrets according to selected profile/google project
- Import/export secret values
- Version control (notification about new published release)

## :sunny: .NET Runtime
This project is built with DotNet 8.0 and is mandatory to install before using.

You can find and install it [here](https://dotnet.microsoft.com/en-us/download/dotnet/8.0).

Verify your dotnet version:

![image](https://user-images.githubusercontent.com/31489258/153608978-cced639e-af42-4485-8c15-5333325b0883.png)

## :gift: Installation

The Installer publishes the code to the machine applications directory and adds it to your system's path.

Installation steps:
1. Download latest release to local machine
2. Un-puck sources (installer and src folders)

- ### Windows
  - Run Installer.exe (as Administrator)

- ### macOS
    - It will be easier to run the installer correctly with the following command, while in its directory (cd command):
```
sudo dotnet Installer.dll
```

3. Reopen terminal / cmd and run
```
gcloud-secret-cli help
```
or
```
gscli help
```
If everything ran smoothly, you should see the list of supported commands.


## :tada: Usage

Before using ensure:
- Installed and configured google cloud sdk
- Installed and configured GKE plugin
- First time user login
```cmd
gcloud auth login <user-name>
```
- You have access to the requested google project
- Optionally you have permission to access to the secret values in the required google project


```cmd
gsclod <command> <profile>
```

FYI - The CLI can be executed using the commands `gscli` or `gclou-secret-cli`.

2 execution modes:
- Interactive (not provided command and profile arguments)
- Not-interactive command execution (exception - command 'edit-profile')

Commands:
- 'create/edit/delete-profile' - profile configuration commands:
  - Mapping to google project id
  - Rules for the creation of environment variable naming settings
- 'get-secrets' - create secrets dump file with values:
  - Connect to google project
  - Get secret ids
  - Access to secret values
  - Dump file
  - Run command 'set-env-var'
- 'clean-env-var' - clean active environment variables
- If you does not have a access to the secret values, use the following commands:
  - 'set/clean-env-var' - sync environment variables with selected profile secrets dump
  - 'import/export-secrets' - import/export accessed secret values


## :gift: New Release Creation Process

- In the new branch to implement fix/new feature
- Update readme
- Update VersionPrefix (major, minor and build numbers) in the file [Directory.Build.props](Directory.Build.props).
- Create and send pull request to review
- After merge into main
- Create a new release:
  - Create new tag contains prefix 'v' and VersionPrefix. Example - 'v1.0.0'
  - Release name is based on created tag name
  - Mark a new release as latest
- Once in day command line check if changed a new latest release and indicate about changes with instructions.


## License

This project is licensed under the [MIT License](LICENSE).
