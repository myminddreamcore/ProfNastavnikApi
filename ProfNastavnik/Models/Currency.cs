using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProfNastavnik.Models;

public partial class Currency
{
    public int IdCurrency { get; set; }

    public string? NameCurrency { get; set; }
    [JsonIgnore]
    public virtual ICollection<Vacancy> Vacancies { get; set; } = new List<Vacancy>();
}
