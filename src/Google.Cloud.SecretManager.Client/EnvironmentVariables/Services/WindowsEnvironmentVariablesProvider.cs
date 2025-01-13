using Microsoft.Win32;

namespace Google.Cloud.SecretManager.Client.EnvironmentVariables.Services;

public class WindowsEnvironmentVariablesProvider : IEnvironmentVariablesProvider
{
    public ISet<string> GetNames(string baseName = null)
    {
        baseName = baseName?.Trim();

        var result = Environment
            .GetEnvironmentVariables()
            .Keys
            .Cast<string>()
            .ToHashSet();

        if (!string.IsNullOrEmpty(baseName))
        {
            result.ExceptWith(result.Where(x => !x.StartsWith(baseName, StringComparison.InvariantCulture)));
        }

        return result;
    }

    public string Get(string name)
    {
        return Environment.GetEnvironmentVariable(name);
    }

    public void Set(string name, string value)
    {
#pragma warning disable CA1416 // The code runs only on Windows
        using var envKey = Registry.CurrentUser.OpenSubKey("Environment", true);
        if (value == null)
        {
            if (envKey.GetValue(name) != null)
            {
                envKey.DeleteValue(name, throwOnMissingValue: false);
            }
        }
        else
        {
            envKey.SetValue(name, value);
        }
#pragma warning restore CA1416 // The code runs only on Windows
    }

    public string CompleteActivationEnvironmentVariables()
    {
        // The below code looks dummy but under the hood it sends event to all apps (and awaits for them to handle it)
        // to update Environment Variables in the process memory. Without this line e.g. explorer.exe won't update itself
        // and no app will have new variables until windows is restarted or explorer.exe will restart itself.
        Environment.SetEnvironmentVariable("AWS_SSM_CLI_ACTIVENAME", Environment.GetEnvironmentVariable("AWS_SSM_CLI_ACTIVENAME"), EnvironmentVariableTarget.User);
        return null;
    }
}