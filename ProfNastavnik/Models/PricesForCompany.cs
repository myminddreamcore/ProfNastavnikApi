using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProfNastavnik.Models;

public partial class PricesForCompany
{
    public int Id { get; set; }

    public string? NamePrice { get; set; }

    public int? CountVacancy { get; set; }

    public int? CountResponce { get; set; }

    public int? Cost { get; set; }

    public string? Description { get; set; }
    [JsonIgnore]
    public virtual ICollection<CompanyPay> CompanyPays { get; set; } = new List<CompanyPay>();
    [JsonIgnore]
    public virtual ICollection<CompanyPrice> CompanyPrices { get; set; } = new List<CompanyPrice>();
}
