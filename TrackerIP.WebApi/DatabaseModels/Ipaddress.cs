using System;
using System.Collections.Generic;

namespace TrackerIP.WebApi.DatabaseModels;

public partial class Ipaddress
{
    public int Id { get; set; }

    public int CountryId { get; set; }

    public string Ip { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Country Country { get; set; } = null!;
}
