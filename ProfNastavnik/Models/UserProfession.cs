using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProfNastavnik.Models;

public partial class UserProfession
{
    public int IdUserProfession { get; set; }

    public int? IdUser { get; set; }

    public int? IdProfession { get; set; }
    [JsonIgnore]
    public virtual Profession? IdProfessionNavigation { get; set; }
    [JsonIgnore]
    public virtual Student? IdUserNavigation { get; set; }
}
