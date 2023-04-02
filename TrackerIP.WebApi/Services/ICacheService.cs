using TrackerIP.Domain.Models;

namespace TrackerIP.WebApi.Services
{
    public interface ICacheService
    {
        public IPDetails? Get(string cacheKey);
        public void Add(string cacheKey, IPDetails value);
        public void Remove(string cacheKey);
    }
}
