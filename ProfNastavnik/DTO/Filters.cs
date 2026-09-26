namespace ProfNastavnik.DTO
{
    public class Filters
    {
        public List<string>? skills { get; set; } = new List<string>();
        public List<string>? sferes { get; set; } = new List<string>();
        public List<string>? formats { get; set; } = new List<string>();
        public int? zenstart { get; set; }
        public int? iduser { get; set; }
        public int? zenEnd { get; set; }
        public bool? user { get; set; }
        public string? name { get; set; }
        public string? city { get; set; }

    }
}
