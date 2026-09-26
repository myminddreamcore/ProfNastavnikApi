using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProfNastavnik.Models;

public partial class UserPrice
{
    public int Id { get; set; }

    public int? IdUser { get; set; }

    public int? IdPrices { get; set; }
    [JsonIgnore]
    public virtual PricesForUser? IdPricesNavigation { get; set; }
    [JsonIgnore]
    public virtual Student? IdUserNavigation { get; set; }
}
