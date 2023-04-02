using TrackerIP.Domain.Models;

namespace TrackerIP.Intergrations;

public interface IIP2CService
{
    public Task<IPDetails?> GetIpAddressDetails(string ipAddress);
}
