using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProfNastavnik.Models;

public partial class RequestsUser
{
    public int IdRequest { get; set; }

    public string? DescriptionRequest { get; set; }

    public int? IdStudent { get; set; }

    public int? IdFeedback { get; set; }

    public string? StatusRequest { get; set; }

    public DateOnly? DateRequest { get; set; }
    [JsonIgnore]
    public virtual FeedbacksCompany? IdFeedbackNavigation { get; set; }
    [JsonIgnore]
    public virtual Student? IdStudentNavigation { get; set; }
}
