using Microsoft.Extensions.Logging;
using TrackerIP.Domain.Models;
using TrackerIP.Intergrations.Mappers;
using TrackerIP.Intergrations.Model;

namespace TrackerIP.Intergrations;

public class IP2CService : IIP2CService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<IIP2CService> _logger;
    private const string PROVIDER_URL = "https://ip2c.org/";

    public IP2CService(HttpClient httpClient, ILogger<IIP2CService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IPDetails?> GetIpAddressDetails(string ipAddress)
    {
        try
        {
            var url = $"{PROVIDER_URL}?ip={ipAddress}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var dataString = await response.Content.ReadAsStringAsync();
            var result = Parse(dataString);
            if (result != null && result.Status == 1)
            {
                return IPDetailsMapper.Map(result);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error on fetch IP infomrmation from provider, IP:{ipAddress}");
            throw new Exception($"An error occurred while fetching IP infomrmation from provider, IP:{ipAddress}", ex);
        }
    }

    private IP2CResponse? Parse(string ipDetails)
    {
        if (string.IsNullOrEmpty(ipDetails)) return null;

        var ipInfo = ipDetails.Split(";");

        return new IP2CResponse()
        {
            Status = int.Parse(ipInfo[0]),
            TwoLetterCode = ipInfo[1],
            ThreeLetterCode = ipInfo[2],
            CountryName = ipInfo[3]
        };
    }
}
