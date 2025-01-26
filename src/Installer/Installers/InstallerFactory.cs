using System.Runtime.InteropServices;
using Installer.Installers.Impl;

namespace Installer.Installers;

public class InstallerFactory
{
    public static IInstaller Get()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return new WindowsInstallerImpl();
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return new OsxInstallerImpl();
        }

        throw new NotSupportedException($"Not supported {RuntimeInformation.RuntimeIdentifier}");
    }
}