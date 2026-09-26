using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProfNastavnik.Models;

public partial class VacanciesFormat
{
    public int IdVacancyFormat { get; set; }

    public int? IdVacancy { get; set; }

    public int? IdFormat { get; set; }
    [JsonIgnore]
    public virtual VacancyFormat? IdFormatNavigation { get; set; }
    [JsonIgnore]
    public virtual Vacancy? IdVacancyNavigation { get; set; }
}
