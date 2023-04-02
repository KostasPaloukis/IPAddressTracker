using Microsoft.EntityFrameworkCore;
using TrackerIP.Domain.Models;
using TrackerIP.WebApi.DatabaseModels;

namespace TrackerIP.WebApi.Repositories;

public class DatabaseRepository : IDataBaseRepository
{
    private readonly TrackerIpdbContext _dbContext;
    private readonly ILogger<DatabaseRepository> _logger;

    public DatabaseRepository(TrackerIpdbContext dbContext, ILogger<DatabaseRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IPDetails?> GetIpAddress(string ip)
    {
        try
        {
            var ipAddress = await _dbContext.Ipaddresses
                .Where(c => c.Ip == ip)
                .Include(c => c.Country)
                .FirstOrDefaultAsync();

            if (ipAddress == null) return null;

            return new IPDetails
            {
                CountryName = ipAddress.Country.Name,
                TwoLetterCode = ipAddress.Country.TwoLetterCode,
                ThreeLetterCode = ipAddress.Country.ThreeLetterCode
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error on fetch IP information from the database");
            throw new Exception("An error occurred while fetching IP information from the database", ex);
        }
    }

    public async Task SaveIpAddress(string ip, IPDetails ipDetails)
    {
        try
        {
            if (ipDetails == null) return;

            var countryId = await RetriveCountryId(ipDetails);

            var idAdress = new Ipaddress
            {
                Ip = ip,
                CountryId = countryId
            };

            await _dbContext.Ipaddresses.AddAsync(idAdress);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error on save IP information to the database");
            throw new Exception("An error occurred while saving IP information to the database", ex);
        }
    }

    public int GetIpAdressesCount()
    {
        try
        {
            return _dbContext.Ipaddresses.Count();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error on fetch total IPs count from the database");
            throw new Exception("An error occurred while fetching total IPs count from the database", ex);
        }
    }

    public IEnumerable<Ipaddress> GetBatchIPs(int skip, int batchSize)
    {
        try
        {
            var ipAddresses = _dbContext.Ipaddresses
              .OrderBy(ip => ip.Id)
              .Skip(skip)
              .Take(batchSize)
              .Include(c => c.Country)
              .ToList();

            return ipAddresses;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error on fetch batch IPs from the database");
            throw new Exception("An error occurred while fetching batch IPs from the database", ex);
        }
    }

    public async Task UpdateIpAddress(Ipaddress ipAddress, IPDetails ipDetails)
    {
        try
        {
            if (ipDetails == null) return;

            var countryId = await RetriveCountryId(ipDetails);
            ipAddress.CountryId = countryId;

            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error on update IP information to the database");
            throw new Exception("An error occurred while updating IP information to the database", ex);
        }
    }

    private async Task<int> RetriveCountryId(IPDetails ipDetails)
    {
        try
        {
            var countryId = 0;
            var countryDetails = await _dbContext.Countries.FirstOrDefaultAsync(c => c.TwoLetterCode == ipDetails.TwoLetterCode);
            if (countryDetails == null)
            {
                var country = new Country
                {
                    Name = ipDetails.CountryName,
                    TwoLetterCode = ipDetails.TwoLetterCode,    
                    ThreeLetterCode = ipDetails.ThreeLetterCode
                };

                await _dbContext.Countries.AddAsync(country);
                await _dbContext.SaveChangesAsync();

                countryId = country.Id;
            }
            else
            {
                if (countryDetails.Name != ipDetails.CountryName ||
                    countryDetails.ThreeLetterCode != ipDetails.ThreeLetterCode)
                {
                    countryDetails.Name = ipDetails.CountryName;
                    countryDetails.ThreeLetterCode = ipDetails.ThreeLetterCode;
                    await _dbContext.SaveChangesAsync();
                }
                countryId = countryDetails.Id;
            }

            return countryId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error on fetch country information from the database");
            throw new Exception("An error occurred while fetching country information from the database", ex);
        }
    }
}
