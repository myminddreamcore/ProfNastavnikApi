using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
namespace ProfNastavnik.Models;

public partial class Adress
{
    public int IdAdress { get; set; }

    public string? CityAdress { get; set; }

    public string? StreetAdress { get; set; }

    public string? FlatAdress { get; set; }

    public string? HouseAdress { get; set; }

    public int? IndexAdress { get; set; }

    public int? IdVacancy { get; set; }
    [JsonIgnore]
    public virtual Vacancy? IdVacancyNavigation { get; set; }
}
