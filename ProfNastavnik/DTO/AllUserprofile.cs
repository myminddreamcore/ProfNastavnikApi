using ProfNastavnik.Models;

namespace ProfNastavnik.DTO
{
    public class AllUserprofile
    {
        public Student? student {  get; set; }
        public string University { get; set; }
        public List<string> Skills { get; set; } = new List<string>();
        public List<string> Proffesions { get; set; } = new List<string>();
        public List<string> StudentFiles { get; set; } = new List<string>();

    }
}
