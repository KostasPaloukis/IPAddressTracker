using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using TrackerIP.Domain.Models;
using TrackerIP.Intergrations;
using TrackerIP.WebApi.DatabaseModels;
using TrackerIP.WebApi.Repositories;
using TrackerIP.WebApi.Services;
using Xunit;

namespace TrackerIP.WebApi.Tests;

public class TrackerIPServiceTests
{
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<IDataBaseRepository> _mockDbRepository;
    private readonly Mock<IIP2CService> _mockIP2CService;
    private readonly Mock<ILogger<TrackerIPService>> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfiguration;

    private const string ipAddress = "103.187.242.7";
    private IPDetails iPDetails = new IPDetails { CountryName = "United States", TwoLetterCode = "US", ThreeLetterCode = "USA" };

    public TrackerIPServiceTests()
    {
        _mockCacheService = new Mock<ICacheService>();
        _mockDbRepository = new Mock<IDataBaseRepository>();
        _mockIP2CService = new Mock<IIP2CService>();
        _mockLogger = new Mock<ILogger<TrackerIPService>>();
        _mockConfiguration = new Mock<IConfiguration>();    
    }

    [Fact]
    public async Task GetIPDetailsAsync_ReturnsFromCache()
    {
        // Arrange
        var expected = iPDetails;
        _mockCacheService.Setup(x => x.Get(ipAddress)).Returns(expected);
        var trackerIPservice = new TrackerIPService(_mockCacheService.Object, _mockDbRepository.Object, _mockIP2CService.Object, _mockConfiguration.Object, _mockLogger.Object);

        // Act
        var actual = await trackerIPservice.GetIPDetailsAsync(ipAddress);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task GetIPDetailsAsync_ReturnsFromDb()
    {
        // Arrange
        var expected = iPDetails;
        _mockCacheService.Setup(x => x.Get(ipAddress)).Returns((IPDetails?)null);
        _mockDbRepository.Setup(x => x.GetIpAddress(ipAddress)).ReturnsAsync(expected);
        var trackerIPservice = new TrackerIPService(_mockCacheService.Object, _mockDbRepository.Object, _mockIP2CService.Object, _mockConfiguration.Object, _mockLogger.Object);

        // Act
        var actual = await trackerIPservice.GetIPDetailsAsync(ipAddress);

        // Assert
        Assert.Equal(expected, actual);
        _mockDbRepository.Verify(x => x.SaveIpAddress(ipAddress, expected), Times.Never);
        _mockCacheService.Verify(x => x.Add(ipAddress, expected), Times.Once);
    }

    [Fact]
    public async Task GetIPDetailsAsync_ReturnsFromProvider()
    {
        // Arrange
        var expected = iPDetails;
        _mockCacheService.Setup(x => x.Get(ipAddress)).Returns((IPDetails?)null);
        _mockDbRepository.Setup(x => x.GetIpAddress(ipAddress)).ReturnsAsync((IPDetails?)null);
        _mockIP2CService.Setup(x => x.GetIpAddressDetails(ipAddress)).ReturnsAsync(expected);
        var trackerIPservice = new TrackerIPService(_mockCacheService.Object, _mockDbRepository.Object, _mockIP2CService.Object, _mockConfiguration.Object, _mockLogger.Object);

        // Act
        var actual = await trackerIPservice.GetIPDetailsAsync(ipAddress);

        // Assert
        Assert.Equal(expected, actual);
        _mockDbRepository.Verify(x => x.SaveIpAddress(ipAddress, expected), Times.Once);
        _mockCacheService.Verify(x => x.Add(ipAddress, expected), Times.Once);
    }

    [Fact]
    public async Task GetIPDetailsAsync_ReturnsNull()
    {
        // Arrange
        _mockCacheService.Setup(x => x.Get(ipAddress)).Returns((IPDetails?)null);
        _mockDbRepository.Setup(x => x.GetIpAddress(ipAddress)).ReturnsAsync((IPDetails?)null);
        _mockIP2CService.Setup(x => x.GetIpAddressDetails(ipAddress)).ReturnsAsync((IPDetails?)null);
        var trackerIPservice = new TrackerIPService(_mockCacheService.Object, _mockDbRepository.Object, _mockIP2CService.Object, _mockConfiguration.Object, _mockLogger.Object);

        // Act
        var actual = await trackerIPservice.GetIPDetailsAsync(ipAddress);

        // Assert
        Assert.Null(actual);
        _mockDbRepository.Verify(x => x.SaveIpAddress(ipAddress, It.IsAny<IPDetails>()), Times.Never);
        _mockCacheService.Verify(x => x.Add(ipAddress, It.IsAny<IPDetails>()), Times.Never);
    }

    [Fact]
    public async Task UpdateIPDetailsAsync_All_IPs_Success()
    {
        // Arrange
        var country = new Country { Name = "Greece", TwoLetterCode = "GR", ThreeLetterCode = "GRC" };
        var ipAddress1 = new Ipaddress { Id = 1, Ip = "10.0.0.1", Country = country };
        var ipAddress2 = new Ipaddress { Id = 2, Ip = "10.0.0.2", Country = country };
        var ipAddress3 = new Ipaddress { Id = 3, Ip = "10.0.0.3", Country = country };

        var ipAddresses = new List<Ipaddress> { ipAddress1, ipAddress2, ipAddress3 };

        var ipDetails1 = new IPDetails { CountryName = "United States", TwoLetterCode = "US", ThreeLetterCode = "USA" };
        var ipDetails2 = new IPDetails { CountryName = "United Kingdom", TwoLetterCode = "GB", ThreeLetterCode = "GBR" };
        var ipDetails3 = new IPDetails { CountryName = "Canada", TwoLetterCode = "CA", ThreeLetterCode = "CAN" };

        _mockDbRepository.Setup(x => x.GetIpAdressesCount()).Returns(ipAddresses.Count);
        _mockDbRepository.Setup(x => x.GetBatchIPs(It.IsAny<int>(), It.IsAny<int>())).Returns(ipAddresses);

        _mockIP2CService.Setup(x => x.GetIpAddressDetails(ipAddress1.Ip)).ReturnsAsync(ipDetails1);
        _mockIP2CService.Setup(x => x.GetIpAddressDetails(ipAddress2.Ip)).ReturnsAsync(ipDetails2);
        _mockIP2CService.Setup(x => x.GetIpAddressDetails(ipAddress3.Ip)).ReturnsAsync(ipDetails3);

        var trackerIPservice = new TrackerIPService(_mockCacheService.Object, _mockDbRepository.Object, _mockIP2CService.Object, _mockConfiguration.Object, _mockLogger.Object);

        // Act
        await trackerIPservice.UpdateIPDetailsAsync();

        // Assert
        _mockCacheService.Verify(x => x.Remove(It.IsAny<string>()), Times.Exactly(3));
        _mockDbRepository.Verify(x => x.UpdateIpAddress(ipAddress1, ipDetails1), Times.Once);
        _mockDbRepository.Verify(x => x.UpdateIpAddress(ipAddress2, ipDetails2), Times.Once);
        _mockDbRepository.Verify(x => x.UpdateIpAddress(ipAddress3, ipDetails3), Times.Once);
    }

    [Fact]
    public async Task UpdateIPDetailsAsync_OnlyIncorrectIPs_Success()
    {
        // Arrange
        var ipAddress1 = new Ipaddress { Id = 1, Ip = "10.0.0.1", Country = new Country { Name = "United States", TwoLetterCode = "US", ThreeLetterCode = "USA" } };
        var ipAddress2 = new Ipaddress { Id = 2, Ip = "10.0.0.2", Country = new Country { Name = "United Kingdom", TwoLetterCode = "GB", ThreeLetterCode = "GBR" } };
        var ipAddress3 = new Ipaddress { Id = 3, Ip = "10.0.0.3", Country = new Country { Name = "Greece", TwoLetterCode = "GR", ThreeLetterCode = "GRC" } };

        var ipAddresses = new List<Ipaddress> { ipAddress1, ipAddress2, ipAddress3 };

        var ipDetails1 = new IPDetails { CountryName = "United States", TwoLetterCode = "US", ThreeLetterCode = "USA" };
        var ipDetails2 = new IPDetails { CountryName = "United Kingdom", TwoLetterCode = "GB", ThreeLetterCode = "GBR" };
        var ipDetails3 = new IPDetails { CountryName = "Canada", TwoLetterCode = "CA", ThreeLetterCode = "CAN" };

        _mockDbRepository.Setup(x => x.GetIpAdressesCount()).Returns(ipAddresses.Count);
        _mockDbRepository.Setup(x => x.GetBatchIPs(It.IsAny<int>(), It.IsAny<int>())).Returns(ipAddresses);

        _mockIP2CService.Setup(x => x.GetIpAddressDetails(ipAddress1.Ip)).ReturnsAsync(ipDetails1);
        _mockIP2CService.Setup(x => x.GetIpAddressDetails(ipAddress2.Ip)).ReturnsAsync(ipDetails2);
        _mockIP2CService.Setup(x => x.GetIpAddressDetails(ipAddress3.Ip)).ReturnsAsync(ipDetails3);

        var trackerIPservice = new TrackerIPService(_mockCacheService.Object, _mockDbRepository.Object, _mockIP2CService.Object, _mockConfiguration.Object, _mockLogger.Object);

        // Act
        await trackerIPservice.UpdateIPDetailsAsync();

        // Assert
        _mockCacheService.Verify(x => x.Remove(It.IsAny<string>()), Times.Once);
        _mockDbRepository.Verify(x => x.UpdateIpAddress(It.IsAny<Ipaddress>(), It.IsAny<IPDetails>()), Times.Once);
    }

    [Fact]
    public async Task UpdateIPDetailsAsync_NoIpAddresses()
    {
        // Arrange
        _mockDbRepository.Setup(x => x.GetIpAdressesCount()).Returns(0);
        var trackerIPservice = new TrackerIPService(_mockCacheService.Object, _mockDbRepository.Object, _mockIP2CService.Object, _mockConfiguration.Object, _mockLogger.Object);

        // Act
        await trackerIPservice.UpdateIPDetailsAsync();

        // Assert
        _mockIP2CService.Verify(x => x.GetIpAddressDetails(It.IsAny<string>()), Times.Never);
        _mockDbRepository.Verify(x => x.UpdateIpAddress(It.IsAny<Ipaddress>(), It.IsAny<IPDetails>()), Times.Never);
        _mockCacheService.Verify(x => x.Remove(It.IsAny<string>()), Times.Never);
    }
}