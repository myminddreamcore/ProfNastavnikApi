using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProfNastavnik.Models;

public partial class Deleting
{
    public int IdDelete { get; set; }

    public string? EmailUser { get; set; }

    public DateOnly? DateDelete { get; set; }
    [JsonIgnore]
    public virtual User? EmailUserNavigation { get; set; }
}
