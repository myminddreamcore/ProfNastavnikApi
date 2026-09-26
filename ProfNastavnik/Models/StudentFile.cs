using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProfNastavnik.Models;

public partial class StudentFile
{
    public int IdStudentFile { get; set; }

    public int? IdUser { get; set; }

    public string? FileUrl { get; set; }
    [JsonIgnore]
    public virtual Student? IdUserNavigation { get; set; }
}
