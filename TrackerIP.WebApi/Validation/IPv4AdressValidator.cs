namespace TrackerIP.WebApi.Validation;

public class IPv4AdressValidator : IValidator<string>
{
    public ValidationResult Validate(string ipAddress)
    {
        var result = new ValidationResult() { IsValid = true };
        if (string.IsNullOrEmpty(ipAddress))
        {
            result.IsValid = false;
            return result;
        }

        string[] ipParts = ipAddress.Split('.');
        if (ipParts.Length != 4)
        {
            result.IsValid = false;
        }

        foreach (string part in ipParts)
        {
            if (!int.TryParse(part, out int value))
            {
                result.IsValid = false;
            }

            if (value < 0 || value > 255)
            {
                result.IsValid = false;
            }
        }

        return result;
    }
}


