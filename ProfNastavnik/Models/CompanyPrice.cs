using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProfNastavnik.Models;

public partial class CompanyPrice
{
    public int Id { get; set; }

    public int? IdCompany { get; set; }

    public int? IdPrice { get; set; }
    [JsonIgnore]
    public virtual Director? IdCompanyNavigation { get; set; }
    [JsonIgnore]
    public virtual PricesForCompany? IdPriceNavigation { get; set; }
}
