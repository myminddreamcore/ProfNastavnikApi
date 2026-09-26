using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProfNastavnik.Models;

public partial class Profession
{
    public int IdProfession { get; set; }

    public string? NameProfession { get; set; }
    [JsonIgnore]
    public virtual ICollection<UserProfession> UserProfessions { get; set; } = new List<UserProfession>();
}
