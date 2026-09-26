using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProfNastavnik.Models;

public partial class Chat
{
    public int IdChat { get; set; }

    public int? IdUser { get; set; }

    public string? TextChat { get; set; }

    public string? StatusChat { get; set; }

    public DateTime? SendAtChat { get; set; }

    public int? IdVacancy { get; set; }

    public int? IdDirector { get; set; }

    public string? EmailAdmin { get; set; }

    public string? SenderChat { get; set; }
    [JsonIgnore]
    public virtual User? EmailAdminNavigation { get; set; }
    [JsonIgnore]
    public virtual Director? IdDirectorNavigation { get; set; }
    [JsonIgnore]
    public virtual Student? IdUserNavigation { get; set; }
    [JsonIgnore]
    public virtual Vacancy? IdVacancyNavigation { get; set; }
}
