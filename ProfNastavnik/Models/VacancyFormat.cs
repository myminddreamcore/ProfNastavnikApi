using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProfNastavnik.Models;

public partial class VacancyFormat
{
    public int IdVacancyFormat { get; set; }

    public string? NameVacancyFormat { get; set; }
    [JsonIgnore]
    public virtual ICollection<VacanciesFormat> VacanciesFormats { get; set; } = new List<VacanciesFormat>();
}
