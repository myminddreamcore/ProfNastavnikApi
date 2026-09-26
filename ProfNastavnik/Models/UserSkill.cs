using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProfNastavnik.Models;

public partial class UserSkill
{
    public int IdUserSkill { get; set; }

    public int? IdUser { get; set; }

    public int? IdSkill { get; set; }
    [JsonIgnore]
    public virtual Skill? IdSkillNavigation { get; set; }
    [JsonIgnore]
    public virtual Student? IdUserNavigation { get; set; }
}
