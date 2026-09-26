using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProfNastavnik.Models;

public partial class PricesForUser
{
    public int IdPrice { get; set; }

    public string? NamePrice { get; set; }

    public int? CountResponces { get; set; }

    public int? CostPrice { get; set; }

    public string? DescriptionPrice { get; set; }
    [JsonIgnore]
    public virtual ICollection<UserPay> UserPays { get; set; } = new List<UserPay>();
    [JsonIgnore]
    public virtual ICollection<UserPrice> UserPrices { get; set; } = new List<UserPrice>();
}
