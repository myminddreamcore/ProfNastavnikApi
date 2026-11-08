using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProfNastavnik.Models;

public partial class Student
{
    public string EmailStudent { get; set; } = null!;

    public int? CourseStudent { get; set; }

    public string? FacultatyStudent { get; set; }

    public int? UniversityStudent { get; set; }

    public DateOnly? BirthdayStudent { get; set; }

    public string? NameStudent { get; set; }

    public string? SurnameStudent { get; set; }

    public string? PatronymicStudent { get; set; }

    public int IdStudent { get; set; }

    public string? StatusStudent { get; set; }

    public DateOnly? DateCreatedStudent { get; set; }

    public TimeOnly? TimeCreatedStudent { get; set; }

    public string? PhotoStudent { get; set; }
    [JsonIgnore]
    public virtual ICollection<Chat> Chats { get; set; } = new List<Chat>();
    [JsonIgnore]
    public virtual User EmailStudentNavigation { get; set; } = null!;
    [JsonIgnore]
    public virtual ICollection<Favourity> Favourities { get; set; } = new List<Favourity>();
    [JsonIgnore]
    public virtual ICollection<FeedbacksCompany> FeedbacksCompanies { get; set; } = new List<FeedbacksCompany>();
    [JsonIgnore]
    public virtual ICollection<FeedbacksUser> FeedbacksUsers { get; set; } = new List<FeedbacksUser>();
    [JsonIgnore]
    public virtual ICollection<Intership> Interships { get; set; } = new List<Intership>();
    [JsonIgnore]
    public virtual ICollection<RequestsUser> RequestsUsers { get; set; } = new List<RequestsUser>();
    [JsonIgnore]
    public virtual ICollection<Response> Responses { get; set; } = new List<Response>();
    [JsonIgnore]
    public virtual ICollection<StudentFile> StudentFiles { get; set; } = new List<StudentFile>();
    [JsonIgnore]
    public virtual University? UniversityStudentNavigation { get; set; }
    [JsonIgnore]
    public virtual ICollection<UserPay> UserPays { get; set; } = new List<UserPay>();
    [JsonIgnore]
    public virtual ICollection<UserPrice> UserPrices { get; set; } = new List<UserPrice>();
    [JsonIgnore]
    public virtual ICollection<UserProfession> UserProfessions { get; set; } = new List<UserProfession>();
    [JsonIgnore]
    public virtual ICollection<UserSkill> UserSkills { get; set; } = new List<UserSkill>();
}
