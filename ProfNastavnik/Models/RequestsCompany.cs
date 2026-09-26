using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProfNastavnik.Models;

public partial class RequestsCompany
{
    public int IdRequest { get; set; }

    public int? IdCompany { get; set; }

    public string? DescriptionRequset { get; set; }

    public int? IdFeedback { get; set; }

    public string? StatusFeedback { get; set; }

    public DateOnly? DateRequest { get; set; }

    [JsonIgnore]
    public virtual Director? IdCompanyNavigation { get; set; }
    [JsonIgnore]
    public virtual FeedbacksUser? IdFeedbackNavigation { get; set; }
}
