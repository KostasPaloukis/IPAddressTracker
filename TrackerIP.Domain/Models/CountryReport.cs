namespace TrackerIP.Domain.Models;

public class CountryReport
{
    public string CountryName { get; set; }
    public int AddressesCount { get; set; }
    public DateTime LastAddressUpdated { get; set; }
}
