using TrackerIP.WebApi.Validation;
using Xunit;

namespace TrackerIP.WebApi.Tests.Validation;

public class IPv4AdressValidatorTests
{
    [Theory]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("192.168.1.256", false)]
    [InlineData("192.-1.1.1", false)]
    [InlineData("192.168.-1.1", false)]
    [InlineData("192.168.1.1", true)]
    public void IPv4AdressValidator_ReturnsCorrectResult(string ipAddress, bool expectedIsValid)
    {
        // Arrange
        var validator = new IPv4AdressValidator();

        // Act
        var result = validator.Validate(ipAddress);

        // Assert
        Assert.Equal(expectedIsValid, result.IsValid);
    }
}
