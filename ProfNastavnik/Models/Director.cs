using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProfNastavnik.Models;

public partial class Director
{
    public int IdDirector { get; set; }

    public string? EmailDirector { get; set; }

    public string? NameDirector { get; set; }

    public string? SurnameDirector { get; set; }

    public string? PatronymicDirector { get; set; }

    public string? NameCompanyDirector { get; set; }

    public string? DescriptionDirector { get; set; }

    public string? WebsiteDirector { get; set; }

    public string? PhoneDirector { get; set; }

    public string? StatusDirector { get; set; }

    public DateOnly? DateCreatedDirector { get; set; }

    public TimeOnly? TimeCreatedDirector { get; set; }

    public string? CityDirector { get; set; }

    public bool? AuthenticationDirector { get; set; }
    [JsonIgnore]
    public virtual ICollection<Chat> Chats { get; set; } = new List<Chat>();
    [JsonIgnore]
    public virtual ICollection<CompanyPay> CompanyPays { get; set; } = new List<CompanyPay>();
    [JsonIgnore]
    public virtual ICollection<CompanyPrice> CompanyPrices { get; set; } = new List<CompanyPrice>();
    [JsonIgnore]
    public virtual ICollection<CompanySfere> CompanySferes { get; set; } = new List<CompanySfere>();
    [JsonIgnore]
    public virtual User? EmailDirectorNavigation { get; set; }
    [JsonIgnore]
    public virtual ICollection<FeedbacksCompany> FeedbacksCompanies { get; set; } = new List<FeedbacksCompany>();
    [JsonIgnore]
    public virtual ICollection<FeedbacksUser> FeedbacksUsers { get; set; } = new List<FeedbacksUser>();
    [JsonIgnore]
    public virtual ICollection<Intership> Interships { get; set; } = new List<Intership>();
    [JsonIgnore]
    public virtual ICollection<Response> Responses { get; set; } = new List<Response>();
    [JsonIgnore]
    public virtual ICollection<Vacancy> Vacancies { get; set; } = new List<Vacancy>();
    [JsonIgnore]
    public virtual ICollection<RequestsCompany> RequestsCompanies { get; set; } = new List<RequestsCompany>();
}
