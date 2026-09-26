using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProfNastavnik.Models;

public partial class VacancySfere
{
    public int IdVacationSfere { get; set; }

    public int? IdVacancy { get; set; }

    public int? IdSfere { get; set; }
    [JsonIgnore]
    public virtual Sfere? IdSfereNavigation { get; set; }
    [JsonIgnore]
    public virtual Vacancy? IdVacancyNavigation { get; set; }
}
