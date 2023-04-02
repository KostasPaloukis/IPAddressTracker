namespace TrackerIP.WebApi.Validation;

public class ValidationFactory : IValidationFactory
{
    public IValidator<T> CreateValidator<T>(ValidationType validationType)
    {
        switch (validationType)
        {
            case ValidationType.IP:
                return new IPv4AdressValidator() as IValidator<T>;
            case ValidationType.CountryCode:
                return new CountryCodeValidator() as IValidator<T>;
            default:
                throw new ArgumentException("Invalid validation type");
        }
    }
}
