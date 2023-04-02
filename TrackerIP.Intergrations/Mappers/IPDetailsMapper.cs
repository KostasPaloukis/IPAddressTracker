using TrackerIP.Domain.Models;
using TrackerIP.Intergrations.Model;

namespace TrackerIP.Intergrations.Mappers;

public static class IPDetailsMapper
{
    public static Func<IP2CResponse, IPDetails> Map = providerResponse =>
       providerResponse == null 
        ? null : 
        new IPDetails 
        {
            CountryName = providerResponse.CountryName.Substring(0, Math.Min(providerResponse.CountryName.Length, 50)),//Name column on Countries table is varchar(50),
            TwoLetterCode = providerResponse.TwoLetterCode, 
            ThreeLetterCode = providerResponse.ThreeLetterCode };

}
