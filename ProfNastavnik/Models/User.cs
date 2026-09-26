using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProfNastavnik.Models;

public partial class User
{
    public string EmailUser { get; set; } = null!;

    public string? PasswordUser { get; set; }

    public string? RoleUser { get; set; }
    [JsonIgnore]
    public virtual ICollection<Chat> Chats { get; set; } = new List<Chat>();
    [JsonIgnore]
    public virtual ICollection<Director> Directors { get; set; } = new List<Director>();
    [JsonIgnore]
    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
    [JsonIgnore]
    public virtual ICollection<Deleting> Deletings { get; set; } = new List<Deleting>();
}
