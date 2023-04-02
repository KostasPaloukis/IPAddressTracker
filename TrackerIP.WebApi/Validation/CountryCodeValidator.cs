using System.Globalization;
using System.Text.RegularExpressions;

namespace TrackerIP.WebApi.Validation;

public class CountryCodeValidator : IValidator<string[]>
{
    public ValidationResult Validate(string[] countryCodes)
    {
        var result = new ValidationResult() { IsValid = true };
        if (countryCodes == null)
        {
            return result;
        }

        foreach (var code in countryCodes)
        {
            if (!Regex.IsMatch(code, "^[A-Z]{2}$"))
            {
                result.IsValid = false;
                break;
            }

            try
            {
                var regionInfo = new RegionInfo(code);
            }
            catch (ArgumentException)
            {
                result.IsValid = false;
                break;
            }
        }

        return result;
    }
}
