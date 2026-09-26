using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProfNastavnik.Models;

public partial class CompanyPay
{
    public int IdCompanyPay { get; set; }

    public int? IdCompany { get; set; }

    public int? IdPay { get; set; }

    public DateOnly? Date { get; set; }
    [JsonIgnore]
    public virtual Director? IdCompanyNavigation { get; set; }
    [JsonIgnore]
    public virtual PricesForCompany? IdPayNavigation { get; set; }
}
