using System.ComponentModel.DataAnnotations;

namespace Google.Cloud.SecretManager.Client.EnvironmentVariables.Helpers;

public static class EnvironmentVariableNameValidationRule
{
    public static char[] InvalidVariableNameCharacters => new []
    {
        '/', '\\', ':', '-', '.', ',', '\'', '"', '`', '{', '}', '[', ']', '$', ';', '(', ')', '@', '#', 
        '^', '?', '!', '&', ' ',
    };

    public static ValidationResult HandlePrefix(string check)
    {
        check = check?.Trim();
        
        if (string.IsNullOrWhiteSpace(check))
        {
            return ValidationResult.Success; //Valid values
        }

        if (check.Length > 10)
        {
            return new ValidationResult("Invalid value - Too long value (exceeded 10 characters)");
        }

        if (InvalidVariableNameCharacters.Any(x => check.Contains(x)))
        {
            return new ValidationResult("Invalid value - Contains invalid character");
        }

        return ValidationResult.Success;
    }
}