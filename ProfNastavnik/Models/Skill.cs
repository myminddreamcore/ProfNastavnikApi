using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProfNastavnik.Models;

public partial class Skill
{
    public int IdSkill { get; set; }

    public string? NameSkill { get; set; }
    [JsonIgnore]
    public virtual ICollection<UserSkill> UserSkills { get; set; } = new List<UserSkill>();
    [JsonIgnore]
    public virtual ICollection<VacancySkill> VacancySkills { get; set; } = new List<VacancySkill>();
}
