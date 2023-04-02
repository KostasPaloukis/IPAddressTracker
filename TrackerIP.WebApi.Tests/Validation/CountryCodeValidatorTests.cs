using TrackerIP.WebApi.Validation;
using Xunit;

namespace TrackerIP.WebApi.Tests.Validation;

public class CountryCodeValidatorTests
{
    [Theory]
    [InlineData(new [] { "US", "GR", "CA" }, true)]
    [InlineData(new [] { "US", "GR", "ca" }, false)]
    [InlineData(new [] { "US", "GR", "CA", "USA" }, false)]
    [InlineData(new [] { "US", "GR", "XX" }, false)]
    [InlineData(null, true)]
    public void IPv4AdressValidator_ReturnsCorrectResult(string[] countryCodes, bool expectedIsValid)
    {
        // Arrange
        var validator = new CountryCodeValidator();

        // Act
        var result = validator.Validate(countryCodes);

        // Assert
        Assert.Equal(expectedIsValid, result.IsValid);
    }
}
