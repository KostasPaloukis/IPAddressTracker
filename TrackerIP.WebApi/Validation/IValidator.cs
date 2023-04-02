namespace TrackerIP.WebApi.Validation;

public interface IValidator<T>
{
    public ValidationResult Validate(T data);
}
