using Microsoft.Extensions.Configuration;

namespace Installer.Installers;

public class InstallerArgs
{
    public required IConfigurationRoot Configuration { get; init; }

    public required CancellationToken CancellationToken { get; init; }
}