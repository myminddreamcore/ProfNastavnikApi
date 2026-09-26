using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProfNastavnik.Models;

public partial class FeedbacksCompany
{
    public int IdFeedbackCompany { get; set; }

    public int? IdUser { get; set; }

    public int? IdCompany { get; set; }

    public string? DescriptionCompany { get; set; }

    public int? IdVacancy { get; set; }

    public string? StatusFeedbackCompany { get; set; }

    public int? RatingStudent { get; set; }

    public DateOnly? DateFeedback { get; set; }
    [JsonIgnore]
    public virtual Director? IdCompanyNavigation { get; set; }
    [JsonIgnore]
    public virtual Student? IdUserNavigation { get; set; }
    [JsonIgnore]
    public virtual Vacancy? IdVacancyNavigation { get; set; }
    [JsonIgnore]
    public virtual ICollection<RequestsUser> RequestsUsers { get; set; } = new List<RequestsUser>();
}
