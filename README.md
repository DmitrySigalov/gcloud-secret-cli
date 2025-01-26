# gclod-secret-cli

[![Build](https://github.com/dmitrysigalov/gclod-secret-manager-cli/workflows/Build/badge.svg)](https://github.com/dmitrysigalov/gclod-secret-manager-cli/actions/workflows/build.yml)
[![License](https://badgen.net/github/license/dmitrysigalov/gclod-secret-manager-cli)](https://github.com/DmitrySigalov/gclod-secret-manager-cli/blob/main/LICENSE)

A dotnet open source which provides integration with google cloud secret manager using tool

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

Guideline:
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

#### New Release Creation Process

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


## :tada: Usage

TODO
