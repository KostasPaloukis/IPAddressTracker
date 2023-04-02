using Microsoft.Data.SqlClient;
using System.Collections.Concurrent;
using System.Data;
using TrackerIP.Domain.Models;
using TrackerIP.Intergrations;
using TrackerIP.WebApi.DatabaseModels;
using TrackerIP.WebApi.Repositories;
using TrackerIP.WebApi.Services;

namespace TrackerIP.WebApi;

public class TrackerIPService : ITrackerIPService
{
    private readonly ILogger<TrackerIPService> _logger;
    private readonly ICacheService _cacheService;
    private readonly IDataBaseRepository _dbRepository;
    private readonly IIP2CService _ip2cService;
    private readonly IConfiguration _configuration;

    public TrackerIPService(
        ICacheService cacheService,
        IDataBaseRepository dbRepository,
        IIP2CService ip2cService,
        IConfiguration configuration,
        ILogger<TrackerIPService> logger)
    {
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService)); 
        _dbRepository = dbRepository ?? throw new ArgumentNullException(nameof(dbRepository));
        _ip2cService = ip2cService ?? throw new ArgumentNullException(nameof(ip2cService));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IPDetails?> GetIPDetailsAsync(string ipAddress)
    {
        try
        {
            var cacheIpDetails = _cacheService.Get(ipAddress);
            if (cacheIpDetails != null) return cacheIpDetails;

            var dbIpDetails = await _dbRepository.GetIpAddress(ipAddress);
            if (dbIpDetails != null)
            {
                _cacheService.Add(ipAddress, dbIpDetails);
                return dbIpDetails;
            }

            var ipDetails = await _ip2cService.GetIpAddressDetails(ipAddress);
            if (ipDetails == null) return null;
            
            await _dbRepository.SaveIpAddress(ipAddress, ipDetails);
            _cacheService.Add(ipAddress, ipDetails);

            return ipDetails;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error on fetch IP details for IP:{ipAddress}");
            throw new Exception($"An error occurred while fetching IP details for IP:{ipAddress}", ex);
        }
    }

    public async Task<IEnumerable<CountryReport>> GetCountryReportAsync(string[] countryCodes)
    {
        try
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                string[]? paramNames = countryCodes?.Select(
                    (s, i) => "@country" + i.ToString())
                    .ToArray();


                var sqlQuery = $@"SELECT Name AS CountryName, COUNT(*) AS AddressesCount, MAX(UpdatedAt) AS LastAddressUpdated
                                FROM Countries
                                INNER JOIN IPAddresses
                                ON Countries.Id = IPAddresses.CountryId ";

                if (countryCodes != null && countryCodes.Length > 0)
                {
                    sqlQuery += $"WHERE TwoLetterCode IN ({string.Join(",", paramNames)}) ";
                }

                sqlQuery += "GROUP BY Name";

                using (var command = new SqlCommand(sqlQuery, connection))
                {
                    for (int i = 0; i < paramNames?.Length; i++)
                    {
                        command.Parameters.AddWithValue(paramNames[i], countryCodes[i]);
                    }

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        var result = new List<CountryReport>();
                        while (await reader.ReadAsync())
                        {
                            result.Add(new CountryReport
                            {
                                CountryName = reader.GetString(0),
                                AddressesCount = reader.GetInt32(1),
                                LastAddressUpdated = reader.GetDateTime(2)
                            });
                        }
                        return result;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error on fetch country report data from database");
            throw new Exception("An error occurred while fetching country report data from database", ex);
        }
    }

    public async Task UpdateIPDetailsAsync()
    {
        try
        {
            int batchSize = 100;
            var totalIpAdresses = _dbRepository.GetIpAdressesCount();
            for (int i = 0; i < totalIpAdresses; i += batchSize)
            {
                var ipAddresses = _dbRepository.GetBatchIPs(i, batchSize);

                var ipDetailsBug = new ConcurrentBag<(Ipaddress, IPDetails)>();

                await Parallel.ForEachAsync(ipAddresses, async (ipAddress, token) =>
                {
                    var ipDetails = await _ip2cService.GetIpAddressDetails(ipAddress.Ip);
                    if (ipDetails != null)
                    {
                        if (ipAddress.Country.Name != ipDetails.CountryName ||
                            ipAddress.Country.TwoLetterCode != ipDetails.TwoLetterCode ||
                            ipAddress.Country.ThreeLetterCode != ipDetails.ThreeLetterCode)
                        {
                            ipDetailsBug.Add((ipAddress, ipDetails));
                        }
                    }
                });

                foreach (var ip in ipDetailsBug)
                {
                    {
                        _cacheService.Remove(ip.Item1.Ip);
                        await _dbRepository.UpdateIpAddress(ip.Item1, ip.Item2);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error on update all IPs inforamations proccess");
            throw new Exception("An error occurred while updating all IPs informations", ex);
        } 
    }
}