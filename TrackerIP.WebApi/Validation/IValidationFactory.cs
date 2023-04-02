namespace TrackerIP.WebApi.Validation;

public interface IValidationFactory
{
    public IValidator<T> CreateValidator<T>(ValidationType validationType);
}
