namespace Installer.Installers;

public interface IInstaller
{
    public Task RunAsync(InstallerArgs args);
}