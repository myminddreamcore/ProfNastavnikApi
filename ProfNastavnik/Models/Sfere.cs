using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProfNastavnik.Models;

public partial class Sfere
{
    public int IdSfere { get; set; }

    public string? NameSfere { get; set; }
    [JsonIgnore]
    public virtual ICollection<CompanySfere> CompanySferes { get; set; } = new List<CompanySfere>();
    [JsonIgnore]
    public virtual ICollection<VacancySfere> VacancySferes { get; set; } = new List<VacancySfere>();
}
