using ProfNastavnik.Models;

namespace ProfNastavnik.DTO
{
    public class CardVacancy
    {
        public Vacancy? Vacancy { get; set; }
        public string? NameCompany { get; set; }
        public int? CompanyId { get; set; }
        public double? RatingCompany { get; set; }
        public List<string>? Formats { get; set; }
        public List<string>? Sferes { get; set; }
        public List<string>? Skills { get; set; }
        public string? Currency { get; set; }
        public string? Adress { get; set; }

    }
}
