using ProfNastavnik.Models;

namespace ProfNastavnik.DTO
{
    public class UserVacancies
    {
        public string NameVacancy { get; set; } = null!;

        public string? NameCompany { get; set; }
        public double? DateEnd { get; set; }
    }
    public class CreateVacancyDTO
    {
        public Vacancy Vacancy { get; set; }
        public Adress Adress { get; set; }
        public List<string>? Skills { get; set; }
        public List<string>? Sferes { get; set; }
        public string? Format { get; set; }
    }
}
