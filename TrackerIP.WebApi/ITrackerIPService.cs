using TrackerIP.Domain.Models;

namespace TrackerIP.WebApi;
public interface ITrackerIPService
{
    public Task<IPDetails?> GetIPDetailsAsync(string ipAddress);
    public Task<IEnumerable<CountryReport>> GetCountryReportAsync(string[] countryCodes);
    public Task UpdateIPDetailsAsync();
}
