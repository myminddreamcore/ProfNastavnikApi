using System;
using System.Collections.Generic;

namespace ProfNastavnik.Models;

public partial class UserConsent
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public string ConsentVersion { get; set; } = null!;

    public string? Ip { get; set; }

    public bool IsAdult { get; set; }

    public DateTime AcceptedAt { get; set; }
}
