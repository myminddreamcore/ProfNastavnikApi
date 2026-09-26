using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProfNastavnik.Models;

public partial class Vacancy
{
    public int IdVacancy { get; set; }

    public int? IdCompany { get; set; }

    public string? NameVacancy { get; set; }

    public string? DescroptionVacancy { get; set; }

    public string? TimeVacancy { get; set; }

    public int? MoneyTypeVacancy { get; set; }

    public bool? AnybodyVacancy { get; set; }

    public string? StatusVacancy { get; set; }

    public int? CountUserVacancy { get; set; }

    public int? ZenStartVacancy { get; set; }

    public int? ZenEndVacancy { get; set; }

    public int? CurrencyVacancy { get; set; }

    public DateOnly? DateCreated { get; set; }
    [JsonIgnore]
    public virtual ICollection<Adress> Adresses { get; set; } = new List<Adress>();
    [JsonIgnore]
    public virtual ICollection<Chat> Chats { get; set; } = new List<Chat>();
    [JsonIgnore]
    public virtual Currency? CurrencyVacancyNavigation { get; set; }
    [JsonIgnore]
    public virtual ICollection<Favourity> Favourities { get; set; } = new List<Favourity>();
    [JsonIgnore]
    public virtual ICollection<FeedbacksCompany> FeedbacksCompanies { get; set; } = new List<FeedbacksCompany>();
    [JsonIgnore]
    public virtual ICollection<FeedbacksUser> FeedbacksUsers { get; set; } = new List<FeedbacksUser>();
    [JsonIgnore]
    public virtual Director? IdCompanyNavigation { get; set; }
    [JsonIgnore]
    public virtual ICollection<Intership> Interships { get; set; } = new List<Intership>();
    [JsonIgnore]
    public virtual MoneyType? MoneyTypeVacancyNavigation { get; set; }
    [JsonIgnore]
    public virtual ICollection<Response> Responses { get; set; } = new List<Response>();
    [JsonIgnore]
    public virtual ICollection<VacanciesFormat> VacanciesFormats { get; set; } = new List<VacanciesFormat>();
    [JsonIgnore]
    public virtual ICollection<VacancySfere> VacancySferes { get; set; } = new List<VacancySfere>();
    [JsonIgnore]
    public virtual ICollection<VacancySkill> VacancySkills { get; set; } = new List<VacancySkill>();
}
