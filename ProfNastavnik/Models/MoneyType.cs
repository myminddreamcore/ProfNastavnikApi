using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProfNastavnik.Models;

public partial class MoneyType
{
    public int IdMoneyType { get; set; }

    public string? NameMoneyType { get; set; }
    [JsonIgnore]
    public virtual ICollection<Vacancy> Vacancies { get; set; } = new List<Vacancy>();
}
