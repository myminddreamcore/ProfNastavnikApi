using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProfNastavnik.Models;

public partial class University
{
    public int IdUniversity { get; set; }

    public string? NameUniversity { get; set; }
    [JsonIgnore]
    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
