# gcloud-secret-cli

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
3. Navigate to installer folder (with terminal 'cd' command)

- ### Windows
  - Run Installer.exe (as Administrator)

- ### macOS
    - It will be easier to run the installer correctly with the following command, while in its directory:
```
sudo dotnet Installer.dll
```

4. Reopen terminal / cmd and run
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
- Optionally you have permission access to the secret values in the required google project

```cmd
gsclod <command> <profile>
```

FYI - The CLI can be executed using the commands `gscli` or `gclou-secret-cli`.

2 execution modes:
- Interactive (not provided command and profile arguments)
- Not-interactive command execution (exception - command 'edit-profile')

### Profile configuration

Profile contains configuration rules:
- Mapping to google project id (by default equals to profile name)
- Settings for the resolving of environment variable names

```json
{
  "ProjectId": "test",
  "SecretIdDelimiter": "_",
  "EnvironmentVariablePrefix": null,
  "RemoveStartDelimiter": true
}
```

## :books: Commands using

### 'create-profile', 'edit-profile' and 'delete-profile'
Using to manage profile configuration which includes

### 'get-secrets'
Using to synchronize environment variables with google project secrets:
- Connect to google project
- Get secret ids
- Access to secret values
- Dump file with values
- Run command 'set-env-var'
#### macOS
For the activation of environment variables required to recreate a process (terminal, Rider, ...)

### 'view-secrets`
Using to see current status of profile:
- Print all secrets
- Synchronization status with environment variables

### 'clean-env-var`
Using to clean environment variables

### If you don't have access to the secret values

#### 'export-secrets' and 'import-secrets`
Using to export/import secrets dump (without access to google project)

#### 'set-env-var'
Using to synchronize environment variables from secrets dump file (without access to google project)


## :gift: New Release Creation Process

- Update VersionPrefix (major, minor and build numbers) in the file [Directory.Build.props](Directory.Build.props).
- Fix unit test
- Create pull request
- For merge pull request use <Squash and merge> option
- In the main branch create a new release:
  - Create new tag contains prefix 'v' and VersionPrefix. Examples - 'v1.1.0'
  - Release name is based on created tag name
  - Mark a new release as latest
- Every day command line check if changed a new latest release and indicate about changes with instructions.


## License

This project is licensed under the [MIT License](LICENSE).
