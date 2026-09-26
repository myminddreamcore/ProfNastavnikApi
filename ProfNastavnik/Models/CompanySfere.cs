using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProfNastavnik.Models;

public partial class CompanySfere
{
    public int IdCompanySfere { get; set; }

    public int? IdCompany { get; set; }

    public int? IdSfere { get; set; }
    [JsonIgnore]
    public virtual Director? IdCompanyNavigation { get; set; }
    [JsonIgnore]
    public virtual Sfere? IdSfereNavigation { get; set; }
}
