using TrackerIP.Domain.Models;
using TrackerIP.Intergrations.Mappers;
using TrackerIP.Intergrations.Model;
using Xunit;

namespace TrackerIP.Intergrations.Tests
{
    public class IPDetailsMapperTests
    {
        [Fact]
        public void MapProviderResponseToIpDetails_NullInput_ReturnsNull()
        {
            // Arrange
            IP2CResponse providerResponse = null;

            // Act
            IPDetails result = IPDetailsMapper.Map(providerResponse);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void MapProviderResponseToIpDetails_ValidInput_ReturnsMappedObject()
        {
            // Arrange
            IP2CResponse providerResponse = new IP2CResponse
            { 
                CountryName = "United States",
                TwoLetterCode = "US",
                ThreeLetterCode = "USA",
                Status = 1
            };

            // Act
            IPDetails result = IPDetailsMapper.Map(providerResponse);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(providerResponse.CountryName, result.CountryName);
            Assert.Equal(providerResponse.TwoLetterCode, result.TwoLetterCode);
            Assert.Equal(providerResponse.ThreeLetterCode, result.ThreeLetterCode);
        }
    }
}