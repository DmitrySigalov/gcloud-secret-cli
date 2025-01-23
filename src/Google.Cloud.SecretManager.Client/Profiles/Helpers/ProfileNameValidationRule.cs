using System.ComponentModel.DataAnnotations;

namespace Google.Cloud.SecretManager.Client.Profiles.Helpers;

public static class ProfileNameValidationRule
{
    public static ValidationResult Handle(
        string check,
        ISet<string> profileNames)
    {
        if (string.IsNullOrWhiteSpace(check))
        {
            return new ValidationResult("Empty value");
        }

        check = check.Trim();

        if (check.Length >= 100)
        {
            return new ValidationResult("Too long name (exceeded 100 characters)");
        }
        
        if (Path.GetInvalidFileNameChars().Any(check.Contains))
        {
            return new ValidationResult("Invalid characters");
        }

        if (profileNames.Any(x => check.Equals(x, StringComparison.InvariantCultureIgnoreCase)))
        {
            return new ValidationResult("Duplicated profile name");
        }
        
        return ValidationResult.Success;
    }
}