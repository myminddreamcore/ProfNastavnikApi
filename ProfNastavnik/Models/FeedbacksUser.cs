using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProfNastavnik.Models;

public partial class FeedbacksUser
{
    public int IdFeedbackUser { get; set; }

    public int? IdUser { get; set; }

    public int? IdCompany { get; set; }

    public string? DescriptionFeedbackUser { get; set; }

    public int? RatingFeedbackUser { get; set; }

    public int? IdVacancy { get; set; }

    public string? StatusFeedbackUser { get; set; }

    public DateOnly? DateFeedback { get; set; }
    [JsonIgnore]
    public virtual Director? IdCompanyNavigation { get; set; }
    [JsonIgnore]
    public virtual Student? IdUserNavigation { get; set; }
    [JsonIgnore]
    public virtual Vacancy? IdVacancyNavigation { get; set; }
    [JsonIgnore]
    public virtual ICollection<RequestsCompany> RequestsCompanies { get; set; } = new List<RequestsCompany>();
}
