using System;
using ProfNastavnik.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfNastavnik.DTO;
using ProfNastavnik.Models;
using System.Net;

using MailKit.Security;
using MimeKit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfNastavnik.DTO;
using ProfNastavnik.Models;
using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Xml.Linq;
using System.Net;
using System.Net.Mail;
namespace ProfNastavnik.Controllers
{
    [Route("api/[controller]")]
    [Route("api/v1/[controller]")]
    [ApiController]
    [TokenAuth]
    public class AdminController : ControllerBase
    {
        [HttpGet("GetAdminMessages/{email}")]
        public ActionResult GetUserMessages(string email)
        {
            try
            {
            using (var context = new ProfnastavnikContext())
            {
                List<Chat> interships = context.Chats.Where(x => x.EmailAdmin == email && (x.SenderChat == "Работодатель" || x.SenderChat == "Стажер") && x.StatusChat != "Прочитано").ToList();

                return Ok(interships.Count);
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "AdminController.GetUserMessages");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        [HttpGet("GetAdminChats/{email}")]
        public ActionResult GetUserChats(string email)
        {
            try
            {
            using (var context = new ProfnastavnikContext())
            {
                var chats = context.Chats
                    .Include(x => x.IdVacancyNavigation)
                    .Include(x => x.IdDirectorNavigation)
                    .Include(x => x.IdUserNavigation)
                    .Where(x => x.EmailAdmin == email)
                    .OrderByDescending(x => x.SendAtChat)
                    .ToList();

                var itog = chats
                    .GroupBy(chat => new {
                        Id = chat.IdDirector ?? chat.IdUser ?? 0,
                        Type = chat.IdDirector != null ? "company" : "user"  
                    })
                    .Select(g => g.First())
                    .Select(x => new ChatDTO
                    {
                        Chat = x,
                        NameCompany = x.IdDirectorNavigation?.NameCompanyDirector,
                        NameVacancy = x.IdVacancyNavigation?.NameVacancy,
                        FIOUser = x.IdUserNavigation != null ?
                            $"{x.IdUserNavigation.SurnameStudent} {x.IdUserNavigation.NameStudent} {x.IdUserNavigation.PatronymicStudent}".Trim() : null
                    })
                    .ToList();

                return itog.Count > 0 ? Ok(itog) : NotFound("чаты не найдены");
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "AdminController.GetUserChats");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        public class TopCompanyDTO
        {
            public int CompanyId { get; set; }
            public string CompanyName { get; set; }
            public double Rating { get; set; }
            public int CompletedInternships { get; set; }
        }

        public class TopStudentAdminDTO
        {
            public int StudentId { get; set; }
            public string FullName { get; set; }
            public string UniversityName { get; set; }
            public int CompletedInternships { get; set; }
            public int SkillsCount { get; set; }
            public int ProfessionsCount { get; set; }
            public string? Photo { get; set; }
        }

        public class TopUniversityDTO
        {
            public int UniversityId { get; set; }
            public string UniversityName { get; set; }
            public int StudentCount { get; set; }
        }

        public class PaymentChartDTO
        {
            public string Month { get; set; }
            public string TariffType { get; set; }
            public double Amount { get; set; }
        }

        public class StudentPaymentChartDTO
        {
            public string Month { get; set; }
            public string TariffType { get; set; }
            public double Amount { get; set; }
        }

        public class CompanyPaymentChartDTO
        {
            public string Month { get; set; }
            public string TariffType { get; set; }
            public double Amount { get; set; }
        }

        public class NewUsersChartDTO
        {
            public string Month { get; set; }
            public int Count { get; set; }
        }

        public class UserChurnChartDTO
        {
            public string Month { get; set; }
            public int Count { get; set; }
        }

        public class SuccessInternshipChartDTO
        {
            public string Month { get; set; }
            public int Count { get; set; }
        }

        public class DashboardStatsDTO
        {
            public int TotalUsers { get; set; }
            public int TotalStudents { get; set; }
            public int TotalCompanies { get; set; }
            public int TotalInternships { get; set; }
            public int CompletedInternships { get; set; }
            public int ActiveInternships { get; set; }
            public double SuccessRate { get; set; }
        }
        [TokenAuth(Roles = "admin")]
        [HttpGet("GetTopCompanies")]
        public async Task<ActionResult<List<TopCompanyDTO>>> GetTopCompanies()
        {
            try
            {
                using var context = new ProfnastavnikContext();

                var companies = await context.Directors
                    .Where(d => d.StatusDirector == "Активен")
                    .Select(d => new TopCompanyDTO
                    {
                        CompanyId = d.IdDirector,
                        CompanyName = d.NameCompanyDirector ?? "Без названия",
                        Rating = context.FeedbacksUsers
                            .Where(r => r.IdCompany == d.IdDirector)
                            .Average(r => r.RatingFeedbackUser) ?? 0,
                        CompletedInternships = context.Interships
                            .Count(i => i.IdCompany == d.IdDirector && i.StatusIntership == "Закончена")
                    })
                    .ToListAsync();

                var result = companies
                    .OrderByDescending(c => (c.Rating * 0.7) + (c.CompletedInternships * 0.3))
                    .Take(3)
                    .ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка: {ex.Message}");
            }
        }

        [TokenAuth(Roles = "admin")]
        [HttpGet("GetTopStudents")]
        public async Task<ActionResult<List<TopStudentAdminDTO>>> GetTopStudents()
        {
            try
            {
                using var context = new ProfnastavnikContext();

                var students = await context.Students
                    .Include(x => x.UniversityStudentNavigation)
                    .Where(s => s.StatusStudent == "Активен")
                    .Select(s => new TopStudentAdminDTO
                    {
                        StudentId = s.IdStudent,
                        FullName = $"{s.SurnameStudent} {s.NameStudent}",
                        UniversityName = s.UniversityStudentNavigation.NameUniversity ?? "Не указан",
                        CompletedInternships = context.Interships
                            .Count(i => i.IdUser == s.IdStudent && i.StatusIntership == "Закончена"),
                        SkillsCount = context.UserSkills
                            .Count(sk => sk.IdUser == s.IdStudent),
                        ProfessionsCount = context.UserProfessions
                            .Count(sp => sp.IdUser == s.IdStudent),
                        Photo = s.PhotoStudent ?? null
                    })
                    .ToListAsync();

                var result = students
                    .OrderByDescending(s =>
                        (s.CompletedInternships * 0.5) +
                        (s.SkillsCount * 0.3) +
                        (s.ProfessionsCount * 0.2))
                    .Take(3)
                    .ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка: {ex.Message}");
            }
        }

        [TokenAuth(Roles = "admin")]
        [HttpGet("GetTopUniversities")]
        public async Task<ActionResult<List<TopUniversityDTO>>> GetTopUniversities()
        {
            try
            {
                using var context = new ProfnastavnikContext();

                var universities = await context.Students
                    .Include(x => x.UniversityStudentNavigation)
                    .Where(s => s.UniversityStudent != null && !string.IsNullOrWhiteSpace(s.UniversityStudentNavigation.NameUniversity))
                    .GroupBy(s => s.UniversityStudentNavigation.NameUniversity) 
                    .Select(g => new TopUniversityDTO
                    {
                        UniversityId = 0,
                        UniversityName = g.Key ?? "Неизвестный университет",
                        StudentCount = g.Count()
                    })
                    .OrderByDescending(u => u.StudentCount)
                    .Take(3)
                    .ToListAsync();

                return Ok(universities);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка: {ex.Message}");
            }
        }

        [TokenAuth(Roles = "admin")]
        [HttpGet("GetPaymentsChart")]
        public async Task<ActionResult<List<PaymentChartDTO>>> GetPaymentsChart()
        {
            try
            {
                using var context = new ProfnastavnikContext();

                var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-11));
                var firstDayOfMonth = new DateOnly(startDate.Year, startDate.Month, 1);

                var companyPayments = await context.CompanyPays.Include(x=>x.IdPayNavigation)
                    .Where(p => p.Date >= firstDayOfMonth)
                    .ToListAsync();

                var studentPayments = await context.UserPays.Include(x=>x.IdPriceNavigation)
                    .Where(p => p.Date >= firstDayOfMonth)
                    .ToListAsync();

                var result = new List<PaymentChartDTO>();

                for (int i = 0; i < 12; i++)
                {
                    var currentMonth = firstDayOfMonth.AddMonths(i);
                    var monthName = currentMonth.ToString("MMM yyyy");

                    var companyAmount = companyPayments
                        .Where(p => p.Date.HasValue &&
                                   p.Date.Value.Month == currentMonth.Month &&
                                   p.Date.Value.Year == currentMonth.Year)
                        .Sum(p => p.IdPayNavigation?.Cost ?? 0);

                    var studentAmount = studentPayments
                        .Where(p => p.Date.HasValue &&
                                   p.Date.Value.Month == currentMonth.Month &&
                                   p.Date.Value.Year == currentMonth.Year)
                        .Sum(p => p.IdPriceNavigation?.CostPrice ?? 0);

                    var totalAmount = companyAmount + studentAmount;

                    if (totalAmount > 0)
                    {
                        result.Add(new PaymentChartDTO
                        {
                            Month = monthName,
                            TariffType = "Все",
                            Amount = totalAmount
                        });
                    }
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка: {ex.Message}");
            }
        }

        [TokenAuth(Roles = "admin")]
        [HttpGet("GetStudentPaymentsChart")]
        public async Task<ActionResult<List<StudentPaymentChartDTO>>> GetStudentPaymentsChart()
        {
            try
            {
                using var context = new ProfnastavnikContext();

                var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-11));
                var firstDayOfMonth = new DateOnly(startDate.Year, startDate.Month, 1);

                var payments = await context.UserPays
                    .Where(p => p.Date >= firstDayOfMonth).Include(x=>x.IdPriceNavigation)
                    .ToListAsync();

                var result = new List<StudentPaymentChartDTO>();

                for (int i = 0; i < 12; i++)
                {
                    var currentMonth = firstDayOfMonth.AddMonths(i);
                    var monthName = currentMonth.ToString("MMM yyyy");

                    var amount = payments
                        .Where(p => p.Date.HasValue &&
                                   p.Date.Value.Month == currentMonth.Month &&
                                   p.Date.Value.Year == currentMonth.Year )
                        .Sum(p => p.IdPriceNavigation?.CostPrice ?? 0);

                    if (amount > 0)
                    {
                        result.Add(new StudentPaymentChartDTO
                        {
                            Month = monthName,
                            TariffType = "Студент",
                            Amount = amount
                        });
                    }
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка: {ex.Message}");
            }
        }

        [TokenAuth(Roles = "admin")]
        [HttpGet("GetCompanyPaymentsChart")]
        public async Task<ActionResult<List<CompanyPaymentChartDTO>>> GetCompanyPaymentsChart()
        {
            try
            {
                using var context = new ProfnastavnikContext();

                var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-11));
                var firstDayOfMonth = new DateOnly(startDate.Year, startDate.Month, 1);

                var payments = await context.CompanyPays.Include(x=>x.IdPayNavigation)
                    .Where(p => p.Date >= firstDayOfMonth)
                    .ToListAsync();

                var result = new List<CompanyPaymentChartDTO>();

                for (int i = 0; i < 12; i++)
                {
                    var currentMonth = firstDayOfMonth.AddMonths(i);
                    var monthName = currentMonth.ToString("MMM yyyy");

                    var amount = payments
                        .Where(p => p.Date.HasValue &&
                                   p.Date.Value.Month == currentMonth.Month &&
                                   p.Date.Value.Year == currentMonth.Year)
                        .Sum(p => p.IdPayNavigation?.Cost ?? 0);

                    if (amount > 0)
                    {
                        result.Add(new CompanyPaymentChartDTO
                        {
                            Month = monthName,
                            TariffType = "Компания",
                            Amount = amount
                        });
                    }
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка: {ex.Message}");
            }
        }

        [TokenAuth(Roles = "admin")]
        [HttpGet("GetNewUsersChart")]
        public async Task<ActionResult<List<NewUsersChartDTO>>> GetNewUsersChart()
        {
            try
            {
                using var context = new ProfnastavnikContext();

                var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-11));
                var firstDayOfMonth = new DateOnly(startDate.Year, startDate.Month, 1);

                var users = await context.Students
                    .Where(u => u.DateCreatedStudent >= firstDayOfMonth)
                    .ToListAsync();

                var result = new List<NewUsersChartDTO>();

                for (int i = 0; i < 12; i++)
                {
                    var currentMonth = firstDayOfMonth.AddMonths(i);
                    var monthName = currentMonth.ToString("MMM yyyy");

                    var count = users
                        .Count(u => u.DateCreatedStudent.Value.Month == currentMonth.Month &&
                                   u.DateCreatedStudent.Value.Year == currentMonth.Year);

                    result.Add(new NewUsersChartDTO
                    {
                        Month = monthName,
                        Count = count
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка: {ex.Message}");
            }
        }

        [TokenAuth(Roles = "admin")]
        [HttpGet("GetUserChurnChart")]
        public async Task<ActionResult<List<UserChurnChartDTO>>> GetUserChurnChart()
        {
            try
            {
                using var context = new ProfnastavnikContext();

                var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-11));
                var firstDayOfMonth = new DateOnly(startDate.Year, startDate.Month, 1);

                var deletedUsers = await context.Deletings
                    .Where(d => d.DateDelete >= firstDayOfMonth)
                    .ToListAsync();

                var result = new List<UserChurnChartDTO>();

                for (int i = 0; i < 12; i++)
                {
                    var currentMonth = firstDayOfMonth.AddMonths(i);
                    var monthName = currentMonth.ToString("MMM yyyy");

                    var count = deletedUsers
                        .Count(d => d.DateDelete.HasValue &&
                                   d.DateDelete.Value.Month == currentMonth.Month &&
                                   d.DateDelete.Value.Year == currentMonth.Year);

                    result.Add(new UserChurnChartDTO
                    {
                        Month = monthName,
                        Count = count
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка: {ex.Message}");
            }
        }
        [TokenAuth(Roles = "admin")]
        [HttpGet("GetSuccessInternshipsChart")]
        public async Task<ActionResult<List<SuccessInternshipChartDTO>>> GetSuccessInternshipsChart()
        {
            try
            {
                using var context = new ProfnastavnikContext();

                var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-11));
                var firstDayOfMonth = new DateOnly(startDate.Year, startDate.Month, 1);

                var internships = await context.Interships
                    .Where(i => i.StatusIntership == "Закончена" && i.DateEndIntership >= firstDayOfMonth)
                    .ToListAsync();

                var result = new List<SuccessInternshipChartDTO>();

                for (int i = 0; i < 12; i++)
                {
                    var currentMonth = firstDayOfMonth.AddMonths(i);
                    var monthName = currentMonth.ToString("MMM yyyy");

                    var count = internships
                        .Count(i => i.DateEndIntership.Value.Month == currentMonth.Month &&
                                   i.DateEndIntership.Value.Year == currentMonth.Year);

                    result.Add(new SuccessInternshipChartDTO
                    {
                        Month = monthName,
                        Count = count
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка: {ex.Message}");
            }
        }

        [TokenAuth(Roles = "admin")]
        [HttpGet("GetDashboardStats")]
        public async Task<ActionResult<DashboardStatsDTO>> GetDashboardStats()
        {
            try
            {
                using var context = new ProfnastavnikContext();

                var totalUsers = await context.Users.CountAsync();
                var totalStudents = await context.Students.CountAsync(s => s.StatusStudent == "Активен");
                var totalCompanies = await context.Directors.CountAsync(d => d.StatusDirector == "Активен");
                var totalInternships = await context.Interships.CountAsync();
                var completedInternships = await context.Interships.CountAsync(i => i.StatusIntership == "Закончена");
                var activeInternships = await context.Interships.CountAsync(i => i.StatusIntership == "Идет");

                var stats = new DashboardStatsDTO
                {
                    TotalUsers = totalUsers,
                    TotalStudents = totalStudents,
                    TotalCompanies = totalCompanies,
                    TotalInternships = totalInternships,
                    CompletedInternships = completedInternships,
                    ActiveInternships = activeInternships,
                    SuccessRate = totalInternships > 0 ? Math.Round((double)completedInternships / totalInternships * 100, 2) : 0
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка: {ex.Message}");
            }
        }

        public class AdminSearchFilters
        {
            public string? Query { get; set; }
            public string? Type { get; set; } 
            public int? Id { get; set; }
        }

        public class AdminSearchResult
        {
            public string Type { get; set; }
            public int Id { get; set; }
            public string Title { get; set; }
            public string? Subtitle { get; set; }
            public string? Description { get; set; }
            public string? Date { get; set; }
            public string? Status { get; set; }
            public string? Photo { get; set; }
        }

        [TokenAuth(Roles = "admin")]
        [HttpPost("Search")]
        public async Task<ActionResult<List<AdminSearchResult>>> Search([FromBody] AdminSearchFilters filters)
        {
            try
            {
                using var context = new ProfnastavnikContext();
                var results = new List<AdminSearchResult>();

                var searchType = filters.Type ?? "all";
                var query = filters.Query?.ToLower() ?? "";

                int? searchId = null;
                if (int.TryParse(query, out int parsedId))
                {
                    searchId = parsedId;
                }

                if (searchId.HasValue && searchId.Value > 0)
                {
                    if (searchType == "all" || searchType == "user")
                    {
                        var user = await context.Students
                            .Where(s => s.IdStudent == searchId.Value)
                            .Select(s => new AdminSearchResult
                            {
                                Type = "user",
                                Id = s.IdStudent,
                                Title = $"{s.SurnameStudent} {s.NameStudent}",
                                Subtitle = s.EmailStudent,
                                Description = s.FacultatyStudent,
                                Status = s.StatusStudent,
                                Date = s.DateCreatedStudent.ToString()
                            })
                            .FirstOrDefaultAsync();

                        if (user != null) results.Add(user);
                    }

                    if (searchType == "all" || searchType == "company")
                    {
                        var company = await context.Directors
                            .Where(d => d.IdDirector == searchId.Value)
                            .Select(d => new AdminSearchResult
                            {
                                Type = "company",
                                Id = d.IdDirector,
                                Title = d.NameCompanyDirector,
                                Subtitle = d.EmailDirector,
                                Description = d.CityDirector,
                                Status = d.StatusDirector,
                                Date = d.DateCreatedDirector.ToString()
                            })
                            .FirstOrDefaultAsync();

                        if (company != null) results.Add(company);
                    }

                    if (searchType == "all" || searchType == "vacancy")
                    {
                        var vacancy = await context.Vacancies
                            .Include(v => v.IdCompanyNavigation)
                            .Where(v => v.IdVacancy == searchId.Value)
                            .Select(v => new AdminSearchResult
                            {
                                Type = "vacancy",
                                Id = v.IdVacancy,
                                Title = v.NameVacancy,
                                Subtitle = v.IdCompanyNavigation.NameCompanyDirector,
                                Description = v.DescroptionVacancy,
                                Status = v.StatusVacancy,
                                Date = v.DateCreated.ToString()
                            })
                            .FirstOrDefaultAsync();

                        if (vacancy != null) results.Add(vacancy);
                    }

                    if (searchType == "all" || searchType == "feedback")
                    {
                        var feedbackCompany = await context.FeedbacksCompanies
                            .Include(f => f.IdCompanyNavigation)
                            .Where(f => f.IdFeedbackCompany == searchId.Value)
                            .Select(f => new AdminSearchResult
                            {
                                Type = "feedback",
                                Id = f.IdFeedbackCompany,
                                Title = "Отзыв о студенте",
                                Subtitle = f.IdCompanyNavigation.NameCompanyDirector,
                                Description = f.DescriptionCompany,
                                Status = f.StatusFeedbackCompany,
                                Date = f.DateFeedback.ToString()
                            })
                            .FirstOrDefaultAsync();

                        if (feedbackCompany != null) results.Add(feedbackCompany);

                        var feedbackUser = await context.FeedbacksUsers
                            .Include(f => f.IdUserNavigation)
                            .Where(f => f.IdFeedbackUser == searchId.Value)
                            .Select(f => new AdminSearchResult
                            {
                                Type = "feedback",
                                Id = f.IdFeedbackUser,
                                Title = "Отзыв о компании",
                                Subtitle = f.IdUserNavigation.EmailStudent,
                                Description = f.DescriptionFeedbackUser,
                                Status = f.StatusFeedbackUser,
                                Date = f.DateFeedback.ToString()
                            })
                            .FirstOrDefaultAsync();

                        if (feedbackUser != null) results.Add(feedbackUser);
                    }

                    return Ok(results);
                }
                if (searchType == "all" || searchType == "user")
                {
                    var users = await context.Students
                        .Where(s => string.IsNullOrEmpty(query) ||
                            s.NameStudent.ToLower().Contains(query) ||
                            s.SurnameStudent.ToLower().Contains(query) ||
                            s.EmailStudent.ToLower().Contains(query) ||
                            s.PatronymicStudent.ToLower().Contains(query))
                        .Take(20)
                        .Select(s => new AdminSearchResult
                        {
                            Type = "user",
                            Id = s.IdStudent,
                            Title = $"{s.SurnameStudent} {s.NameStudent}",
                            Subtitle = s.EmailStudent,
                            Description = s.FacultatyStudent,
                            Status = s.StatusStudent,
                            Date = s.DateCreatedStudent.ToString()
                        })
                        .ToListAsync();

                    results.AddRange(users);
                }

                if (searchType == "all" || searchType == "company")
                {
                    var companies = await context.Directors
                        .Where(d => string.IsNullOrEmpty(query) ||
                            d.NameCompanyDirector.ToLower().Contains(query) ||
                            d.EmailDirector.ToLower().Contains(query) ||
                            d.CityDirector.ToLower().Contains(query))
                        .Take(20)
                        .Select(d => new AdminSearchResult
                        {
                            Type = "company",
                            Id = d.IdDirector,
                            Title = d.NameCompanyDirector,
                            Subtitle = d.EmailDirector,
                            Description = d.CityDirector,
                            Status = d.StatusDirector,
                            Date = d.DateCreatedDirector.ToString()
                        })
                        .ToListAsync();

                    results.AddRange(companies);
                }
                if (searchType == "all" || searchType == "vacancy")
                {
                    var vacancies = await context.Vacancies
                        .Include(v => v.IdCompanyNavigation)
                        .Where(v => string.IsNullOrEmpty(query) ||
                            v.NameVacancy.ToLower().Contains(query) ||
                            v.DescroptionVacancy.ToLower().Contains(query))
                        .Take(20)
                        .Select(v => new AdminSearchResult
                        {
                            Type = "vacancy",
                            Id = v.IdVacancy,
                            Title = v.NameVacancy,
                            Subtitle = v.IdCompanyNavigation.NameCompanyDirector,
                            Description = v.DescroptionVacancy,
                            Status = v.StatusVacancy,
                            Date = v.DateCreated.ToString()
                        })
                        .ToListAsync();

                    results.AddRange(vacancies);
                }

                if (searchType == "all" || searchType == "feedback")
                {
                    var feedbacksToUsers = await context.FeedbacksCompanies
                        .Include(f => f.IdCompanyNavigation)
                        .Where(f => string.IsNullOrEmpty(query) ||
                            f.DescriptionCompany.ToLower().Contains(query))
                        .Take(10)
                        .Select(f => new AdminSearchResult
                        {
                            Type = "feedback",
                            Id = f.IdFeedbackCompany,
                            Title = "Отзыв о студенте",
                            Subtitle = f.IdCompanyNavigation.NameCompanyDirector,
                            Description = f.DescriptionCompany,
                            Status = f.StatusFeedbackCompany,
                            Date = f.DateFeedback.ToString()
                        })
                        .ToListAsync();

                    results.AddRange(feedbacksToUsers);

                    var feedbacksToCompanies = await context.FeedbacksUsers
                        .Include(f => f.IdUserNavigation)
                        .Where(f => string.IsNullOrEmpty(query) ||
                            f.DescriptionFeedbackUser.ToLower().Contains(query))
                        .Take(10)
                        .Select(f => new AdminSearchResult
                        {
                            Type = "feedback",
                            Id = f.IdFeedbackUser,
                            Title = "Отзыв о компании",
                            Subtitle = f.IdUserNavigation.EmailStudent,
                            Description = f.DescriptionFeedbackUser,
                            Status = f.StatusFeedbackUser,
                            Date = f.DateFeedback.ToString()
                        })
                        .ToListAsync();

                    results.AddRange(feedbacksToCompanies);
                }

                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка: {ex.Message}");
            }
        }
        [TokenAuth(Roles = "admin")]
        [HttpPut("UpdateUserStatus/{userId}/{status}")]
        public async Task<ActionResult> UpdateUserStatus(int userId, string status)
        {
            try
            {
                using var context = new ProfnastavnikContext();
                var user = await context.Students
                    .Include(x => x.EmailStudentNavigation)
                    .FirstOrDefaultAsync(s => s.IdStudent == userId);

                if (user == null) return NotFound("Пользователь не найден");

                user.StatusStudent = status;
                await context.SaveChangesAsync();

                try
                {
                    string subject = status switch
                    {
                        "Активен" => "Ваш аккаунт активирован!",
                        "Удален" => "Ваш аккаунт заблокирован",
                        "На модерации" => "Ваш аккаунт на модерации",
                        _ => "Изменение статуса аккаунта"
                    };

                    string body = status switch
                    {
                        "Активен" => $@"
                    <h2>Здравствуйте, {user.NameStudent}!</h2>
                    <p>Ваш аккаунт был активирован.</p>
                    <p>Теперь вы можете полноценно пользоваться всеми функциями платформы.</p>
                    <p>С уважением,<br>Команда Воркшип</p>",
                        "Удален" => $@"
                    <h2>Здравствуйте, {user.NameStudent}!</h2>
                    <p>Ваш аккаунт был заблокирован за нарушение правил сообщества.</p>
                    <p>Если вы считаете это ошибкой, свяжитесь с поддержкой: official.workship@mail.ru</p>
                    <p>С уважением,<br>Команда Воркшип</p>",
                        "На модерации" => $@"
                    <h2>Здравствуйте, {user.NameStudent}!</h2>
                    <p>Ваш аккаунт отправлен на модерацию.</p>
                    <p>Мы проверим ваши данные и свяжемся с вами в ближайшее время.</p>
                    <p>С уважением,<br>Команда Воркшип</p>",
                        _ => $@"
                    <h2>Здравствуйте, {user.NameStudent}!</h2>
                    <p>Статус вашего аккаунта изменен на: <strong>{status}</strong></p>
                    <p>С уважением,<br>Команда Воркшип</p>"
                    };

                    SendEmail(user.EmailStudent, subject, body);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка отправки уведомления: {ex.Message}");
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка: {ex.Message}");
            }
        }

        [TokenAuth(Roles = "admin")]
        [HttpPut("UpdateCompanyStatus/{companyId}/{status}")]
        public async Task<ActionResult> UpdateCompanyStatus(int companyId, string status)
        {
            try
            {
                using var context = new ProfnastavnikContext();
                var company = await context.Directors
                    .Include(x => x.EmailDirectorNavigation)
                    .FirstOrDefaultAsync(d => d.IdDirector == companyId);

                if (company == null) return NotFound("Компания не найдена");

                company.StatusDirector = status;
                await context.SaveChangesAsync();

                try
                {
                    string subject = status switch
                    {
                        "Активен" => "Ваша компания активирована!",
                        "Удален" => "Ваша компания заблокирована",
                        "На модерации" => "Ваша компания на модерации",
                        _ => "Изменение статуса компании"
                    };

                    string body = status switch
                    {
                        "Активен" => $@"
                    <h2>Здравствуйте!</h2>
                    <p>Ваша компания <strong>«{company.NameCompanyDirector}»</strong> была активирована.</p>
                    <p>Теперь вы можете публиковать вакансии и находить лучших стажеров.</p>
                    <p>С уважением,<br>Команда Воркшип</p>",
                        "Удален" => $@"
                    <h2>Здравствуйте!</h2>
                    <p>Ваша компания <strong>«{company.NameCompanyDirector}»</strong> была заблокирована за нарушение правил сообщества.</p>
                    <p>Если вы считаете это ошибкой, свяжитесь с поддержкой: official.workship@mail.ru</p>
                    <p>С уважением,<br>Команда Воркшип</p>",
                        "На модерации" => $@"
                    <h2>Здравствуйте!</h2>
                    <p>Ваша компания <strong>«{company.NameCompanyDirector}»</strong> отправлена на модерацию.</p>
                    <p>Мы проверим ваши данные и свяжемся с вами в ближайшее время.</p>
                    <p>С уважением,<br>Команда Воркшип</p>",
                        _ => $@"
                    <h2>Здравствуйте!</h2>
                    <p>Статус вашей компании <strong>«{company.NameCompanyDirector}»</strong> изменен на: <strong>{status}</strong></p>
                    <p>С уважением,<br>Команда Воркшип</p>"
                    };

                    SendEmail(company.EmailDirector, subject, body);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка отправки уведомления: {ex.Message}");
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка: {ex.Message}");
            }
        }

        [TokenAuth(Roles = "admin")]
        [HttpPut("UpdateVacancyStatus/{vacancyId}/{status}")]
        public async Task<ActionResult> UpdateVacancyStatus(int vacancyId, string status)
        {
            try
            {
                using var context = new ProfnastavnikContext();
                var vacancy = await context.Vacancies
                    .Include(x => x.IdCompanyNavigation)
                    .ThenInclude(x => x.EmailDirectorNavigation)
                    .FirstOrDefaultAsync(v => v.IdVacancy == vacancyId);

                if (vacancy == null) return NotFound("Вакансия не найдена");

                vacancy.StatusVacancy = status;
                await context.SaveChangesAsync();

                if (vacancy.IdCompanyNavigation != null)
                {
                    try
                    {
                        string subject = status switch
                        {
                            "Активна" => "Ваша вакансия опубликована!",
                            "Архив" => "Ваша вакансия отправлена в архив",
                            "На модерации" => "Ваша вакансия на модерации",
                            _ => "Изменение статуса вакансии"
                        };

                        string body = status switch
                        {
                            "Активна" => $@"
                        <h2>Здравствуйте!</h2>
                        <p>Ваша вакансия <strong>«{vacancy.NameVacancy}»</strong> опубликована и доступна для просмотра.</p>
                        <p>Желаем вам найти лучшего кандидата!</p>
                        <p>С уважением,<br>Команда Воркшип</p>",
                            "Архив" => $@"
                        <h2>Здравствуйте!</h2>
                        <p>Ваша вакансия <strong>«{vacancy.NameVacancy}»</strong> была отправлена в архив.</p>
                        <p>Вы можете восстановить её в любой момент.</p>
                        <p>С уважением,<br>Команда Воркшип</p>",
                            "На модерации" => $@"
                        <h2>Здравствуйте!</h2>
                        <p>Ваша вакансия <strong>«{vacancy.NameVacancy}»</strong> отправлена на модерацию.</p>
                        <p>Мы проверим её и свяжемся с вами в ближайшее время.</p>
                        <p>С уважением,<br>Команда Воркшип</p>",
                            _ => $@"
                        <h2>Здравствуйте!</h2>
                        <p>Статус вашей вакансии <strong>«{vacancy.NameVacancy}»</strong> изменен на: <strong>{status}</strong></p>
                        <p>С уважением,<br>Команда Воркшип</p>"
                        };

                        SendEmail(vacancy.IdCompanyNavigation.EmailDirector, subject, body);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка отправки уведомления: {ex.Message}");
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка: {ex.Message}");
            }
        }

        [TokenAuth(Roles = "admin")]
        [HttpPut("UpdateFeedbackCompanyStatus/{feedbackId}/{status}")]
        public async Task<ActionResult> UpdateFeedbackCompanyStatus(int feedbackId, string status)
        {
            try
            {
                using var context = new ProfnastavnikContext();
                var feedback = await context.FeedbacksCompanies
                    .Include(x => x.IdUserNavigation)
                    .ThenInclude(x => x.EmailStudentNavigation)
                    .FirstOrDefaultAsync(f => f.IdFeedbackCompany == feedbackId);

                if (feedback == null) return NotFound("Отзыв не найден");

                feedback.StatusFeedbackCompany = status;
                await context.SaveChangesAsync();

                if (feedback.IdUserNavigation != null)
                {
                    try
                    {
                        string subject = status switch
                        {
                            "Одобрено" => "Ваш отзыв одобрен!",
                            "Удален" => "Ваш отзыв удален",
                            "На модерации" => "Ваш отзыв на модерации",
                            _ => "Изменение статуса отзыва"
                        };

                        string body = status switch
                        {
                            "Одобрено" => $@"
                        <h2>Здравствуйте, {feedback.IdUserNavigation.NameStudent}!</h2>
                        <p>Ваш отзыв о компании был одобрен и опубликован.</p>
                        <p>Спасибо, что делитесь своим опытом!</p>
                        <p>С уважением,<br>Команда Воркшип</p>",
                            "Удалено" => $@"
                        <h2>Здравствуйте, {feedback.IdUserNavigation.NameStudent}!</h2>
                        <p>Ваш отзыв был удален за нарушение правил сообщества.</p>
                        <p>Если вы считаете это ошибкой, свяжитесь с поддержкой: official.workship@mail.ru</p>
                        <p>С уважением,<br>Команда Воркшип</p>",
                            "На модерации" => $@"
                        <h2>Здравствуйте, {feedback.IdUserNavigation.NameStudent}!</h2>
                        <p>Ваш отзыв отправлен на модерацию.</p>
                        <p>Мы проверим его и свяжемся с вами в ближайшее время.</p>
                        <p>С уважением,<br>Команда Воркшип</p>",
                            _ => $@"
                        <h2>Здравствуйте!</h2>
                        <p>Статус вашего отзыва изменен на: <strong>{status}</strong></p>
                        <p>С уважением,<br>Команда Воркшип</p>"
                        };

                        SendEmail(feedback.IdUserNavigation.EmailStudent, subject, body);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка отправки уведомления: {ex.Message}");
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка: {ex.Message}");
            }
        }

        [TokenAuth(Roles = "admin")]
        [HttpPut("UpdateFeedbackUserStatus/{feedbackId}/{status}")]
        public async Task<ActionResult> UpdateFeedbackUserStatus(int feedbackId, string status)
        {
            try
            {
                using var context = new ProfnastavnikContext();
                var feedback = await context.FeedbacksUsers
                    .Include(x => x.IdCompanyNavigation)
                    .ThenInclude(x => x.EmailDirectorNavigation)
                    .FirstOrDefaultAsync(f => f.IdFeedbackUser == feedbackId);

                if (feedback == null) return NotFound("Отзыв не найден");

                feedback.StatusFeedbackUser = status;
                await context.SaveChangesAsync();

                if (feedback.IdCompanyNavigation != null)
                {
                    try
                    {
                        string subject = status switch
                        {
                            "Одобрен" => "Ваш отзыв одобрен!",
                            "Удален" => "Ваш отзыв удален",
                            "На модерации" => "Ваш отзыв на модерации",
                            _ => "Изменение статуса отзыва"
                        };

                        string body = status switch
                        {
                            "Одобрен" => $@"
                        <h2>Здравствуйте!</h2>
                        <p>Ваш отзыв о студенте был одобрен и опубликован.</p>
                        <p>Спасибо, что делитесь своим опытом!</p>
                        <p>С уважением,<br>Команда Воркшип</p>",
                            "Удален" => $@"
                        <h2>Здравствуйте!</h2>
                        <p>Ваш отзыв был удален за нарушение правил сообщества.</p>
                        <p>Если вы считаете это ошибкой, свяжитесь с поддержкой: official.workship@mail.ru</p>
                        <p>С уважением,<br>Команда Воркшип</p>",
                            "На модерации" => $@"
                        <h2>Здравствуйте!</h2>
                        <p>Ваш отзыв отправлен на модерацию.</p>
                        <p>Мы проверим его и свяжемся с вами в ближайшее время.</p>
                        <p>С уважением,<br>Команда Воркшип</p>",
                            _ => $@"
                        <h2>Здравствуйте!</h2>
                        <p>Статус вашего отзыва изменен на: <strong>{status}</strong></p>
                        <p>С уважением,<br>Команда Воркшип</p>"
                        };

                        SendEmail(feedback.IdCompanyNavigation.EmailDirector, subject, body);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка отправки уведомления: {ex.Message}");
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка: {ex.Message}");
            }
        }

        private void SendEmail(string toEmail, string subject, string body)
        {
            try
            {
                string fromEmail = "official.workship@mail.ru";
                string password = "SSIuL9fn3p0HSDFSjTtW";
                string smtpServer = "smtp.mail.ru";
                int port = 587;

                using (var client = new SmtpClient(smtpServer, port))
                {
                    client.EnableSsl = true;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(fromEmail, password);
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.Timeout = 30000;

                    using (var mail = new MailMessage())
                    {
                        mail.From = new MailAddress(fromEmail, "Воркшип");
                        mail.To.Add(toEmail);
                        mail.Subject = subject;
                        mail.Body = body;
                        mail.IsBodyHtml = true;

                        client.Send(mail);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка отправки письма: {ex.Message}");
            }
        }

        public class ModerationItems
        {
            public List<ModerationUser> Users { get; set; } = new();
            public List<ModerationCompany> Companies { get; set; } = new();
            public List<ModerationVacancy> Vacancies { get; set; } = new();
            public List<ModerationFeedbackCompany> FeedbacksToUsers { get; set; } = new();
            public List<ModerationFeedbackUser> FeedbacksToCompanies { get; set; } = new();
        }

        public class ModerationUser
        {
            public int Id { get; set; }
            public string FullName { get; set; }
            public string Email { get; set; }
            public string University { get; set; }
            public int? Course { get; set; }
            public string Status { get; set; }
        }

        public class ModerationCompany
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public string City { get; set; }
            public string Status { get; set; }
        }

        public class ModerationVacancy
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string CompanyName { get; set; }
            public string Description { get; set; }
            public string Status { get; set; }
        }

        public class ModerationFeedbackCompany
        {
            public int Id { get; set; }
            public string StudentName { get; set; }
            public string CompanyName { get; set; }
            public string Description { get; set; }
            public int Rating { get; set; }
            public string Status { get; set; }
            public string Date { get; set; }
        }

        public class ModerationFeedbackUser
        {
            public int Id { get; set; }
            public string StudentName { get; set; }
            public string CompanyName { get; set; }
            public string Description { get; set; }
            public int Rating { get; set; }
            public string Status { get; set; }
            public string Date { get; set; }
        }

        [TokenAuth(Roles = "admin")]
        [HttpGet("GetModerationItems")]
        public async Task<ActionResult<ModerationItems>> GetModerationItems()
        {
            try
            {
                using var context = new ProfnastavnikContext();
                var result = new ModerationItems();

                result.Users = await context.Students
                    .Where(s => s.StatusStudent == "На модерации")
                    .Select(s => new ModerationUser
                    {
                        Id = s.IdStudent,
                        FullName = $"{s.SurnameStudent} {s.NameStudent}",
                        Email = s.EmailStudent,
                        University = context.Universities
                            .Where(u => u.IdUniversity == s.UniversityStudent)
                            .Select(u => u.NameUniversity)
                            .FirstOrDefault(),
                        Course = s.CourseStudent,
                        Status = s.StatusStudent
                    })
                    .ToListAsync();

                result.Companies = await context.Directors
                    .Where(d => d.StatusDirector == "На модерации")
                    .Select(d => new ModerationCompany
                    {
                        Id = d.IdDirector,
                        Name = d.NameCompanyDirector,
                        Email = d.EmailDirector,
                        City = d.CityDirector,
                        Status = d.StatusDirector
                    })
                    .ToListAsync();

                result.Vacancies = await context.Vacancies
                    .Include(v => v.IdCompanyNavigation)
                    .Where(v => v.StatusVacancy == "На модерации")
                    .Select(v => new ModerationVacancy
                    {
                        Id = v.IdVacancy,
                        Name = v.NameVacancy,
                        CompanyName = v.IdCompanyNavigation.NameCompanyDirector,
                        Description = v.DescroptionVacancy,
                        Status = v.StatusVacancy
                    })
                    .ToListAsync();
                result.FeedbacksToUsers = await context.FeedbacksCompanies
                .Include(f => f.IdCompanyNavigation)
                .Include(f => f.IdUserNavigation)
                .Where(f => f.StatusFeedbackCompany == "На модерации")
                .Select(f => new ModerationFeedbackCompany
                {
                    Id = f.IdFeedbackCompany,
                    StudentName = $"{f.IdUserNavigation.SurnameStudent} {f.IdUserNavigation.NameStudent}",
                    CompanyName = f.IdCompanyNavigation.NameCompanyDirector,
                    Description = f.DescriptionCompany,
                    Rating = f.RatingStudent ?? 0,
                    Status = f.StatusFeedbackCompany,
                    Date = f.DateFeedback.HasValue ? f.DateFeedback.Value.ToString("dd.MM.yyyy") : null
                })
                .ToListAsync();

                result.FeedbacksToCompanies = await context.FeedbacksUsers
                    .Include(f => f.IdCompanyNavigation)
                    .Include(f => f.IdUserNavigation)
                    .Where(f => f.StatusFeedbackUser == "На модерации")
                    .Select(f => new ModerationFeedbackUser
                    {
                        Id = f.IdFeedbackUser,
                        StudentName = $"{f.IdUserNavigation.SurnameStudent} {f.IdUserNavigation.NameStudent}",
                        CompanyName = f.IdCompanyNavigation.NameCompanyDirector,
                        Description = f.DescriptionFeedbackUser,
                        Rating = f.RatingFeedbackUser ?? 0,
                        Status = f.StatusFeedbackUser,
                        Date = f.DateFeedback.HasValue ? f.DateFeedback.Value.ToString("dd.MM.yyyy") : null
                    })
                    .ToListAsync();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка: {ex.Message}");
            }
        }
    }
}