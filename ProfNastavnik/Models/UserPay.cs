using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProfNastavnik.Models;

public partial class UserPay
{
    public int IdUserPay { get; set; }

    public int? IdUser { get; set; }

    public int? IdPrice { get; set; }

    public DateOnly? Date { get; set; }
    [JsonIgnore]
    public virtual PricesForUser? IdPriceNavigation { get; set; }
    [JsonIgnore]
    public virtual Student? IdUserNavigation { get; set; }
}
