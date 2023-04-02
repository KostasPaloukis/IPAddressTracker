using TrackerIP.Domain.Models;
using TrackerIP.WebApi.DatabaseModels;

namespace TrackerIP.WebApi.Repositories;

public interface IDataBaseRepository
{
    public Task<IPDetails?> GetIpAddress(string ip);
    public Task SaveIpAddress(string ip, IPDetails ipDetails);
    public IEnumerable<Ipaddress> GetBatchIPs(int skip, int batchSize);
    public int GetIpAdressesCount();
    public Task UpdateIpAddress(Ipaddress ipAdress, IPDetails ipDetails);
}
