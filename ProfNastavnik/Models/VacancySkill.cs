using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProfNastavnik.Models;

public partial class VacancySkill
{
    public int IdVacationSkill { get; set; }

    public int? IdVacancy { get; set; }

    public int? IdSkill { get; set; }
    [JsonIgnore]
    public virtual Skill? IdSkillNavigation { get; set; }
    [JsonIgnore]
    public virtual Vacancy? IdVacancyNavigation { get; set; }
}
