using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProfNastavnik.Models;

public partial class Favourity
{
    public int IdFavourity { get; set; }

    public int? IdUser { get; set; }

    public int? IdVacancy { get; set; }
    [JsonIgnore]
    public virtual Student? IdUserNavigation { get; set; }
    [JsonIgnore]
    public virtual Vacancy? IdVacancyNavigation { get; set; }
}
