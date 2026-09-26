using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProfNastavnik.Models;

public partial class Intership
{
    public int IdIntership { get; set; }

    public int? IdUser { get; set; }

    public int? IdCompany { get; set; }

    public DateOnly? DateStartIntership { get; set; }

    public DateOnly? DateEndIntership { get; set; }

    public string? StatusIntership { get; set; }

    public int? ResponseId { get; set; }

    public int? IdVacancy { get; set; }
    [JsonIgnore]
    public virtual Director? IdCompanyNavigation { get; set; }
    [JsonIgnore]
    public virtual Student? IdUserNavigation { get; set; }
    [JsonIgnore]
    public virtual Vacancy? IdVacancyNavigation { get; set; }
    [JsonIgnore]
    public virtual Response? Response { get; set; }
}
