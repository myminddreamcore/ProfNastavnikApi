using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProfNastavnik.Models;

public partial class Response
{
    public int IdResponse { get; set; }

    public int? IdCompany { get; set; }

    public int? IdUser { get; set; }

    public string? DescriptionResponse { get; set; }

    public string? PortfolioResponse { get; set; }

    public string? StatusResponse { get; set; }

    public int? IdVacancy { get; set; }

    public DateOnly? DateResponse { get; set; }
    [JsonIgnore]
    public virtual Director? IdCompanyNavigation { get; set; }
    [JsonIgnore]
    public virtual Student? IdUserNavigation { get; set; }
    [JsonIgnore]
    public virtual Vacancy? IdVacancyNavigation { get; set; }
    [JsonIgnore]
    public virtual ICollection<Intership> Interships { get; set; } = new List<Intership>();
}
