using System;
using System.Collections.Generic;

namespace ProfNastavnik.Models;

public partial class Deleting
{
    public int IdDelete { get; set; }

    public string? EmailUser { get; set; }

    public DateOnly? DateDelete { get; set; }

    public virtual User? EmailUserNavigation { get; set; }
}
