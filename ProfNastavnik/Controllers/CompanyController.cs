using System;
using ProfNastavnik.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfNastavnik.DTO;
using ProfNastavnik.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ProfNastavnik.Controllers
{
    [Route("api/[controller]")]
    [Route("api/v1/[controller]")]
    [ApiController]
    [TokenAuth]
    public class CompanyController : ControllerBase
    {
       

        // GET api/<CompanyController>/5
        [HttpGet("GetCompanyCard/{id}")]
        public ActionResult Get(int id)
        {
            try
            {
            using (var context = new ProfnastavnikContext()) 
            {
                Director directory = context.Directors.FirstOrDefault(x=>x.IdDirector==id);
                if (directory != null)
                {
                    DirectorDTO itog = new DirectorDTO()
                    {
                        director = directory,
                        rating = context.FeedbacksUsers.Where(x => x.IdCompany == directory.IdDirector &&x.StatusFeedbackUser!="Удален")?.Count() > 0 ? context.FeedbacksUsers.Where(x => x.IdCompany == directory.IdDirector&&x.StatusFeedbackUser!="Удален")?.Sum(x => x.RatingFeedbackUser) / context.FeedbacksUsers.Where(x => x.IdCompany == directory.IdDirector && x.StatusFeedbackUser != "Удален")?.Count() : 0,

                    };
                    return Ok(itog);
                }
                return NotFound();
             }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "CompanyController.Get");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        [HttpGet("GetCompany/{email}")]
        public ActionResult GetUser(string email)
        {
            try
            {
            using (var context = new ProfnastavnikContext())
            {
                Director user = context.Directors.FirstOrDefault(x => x.EmailDirector == email);
                return user != null ? Ok(user) : NotFound("компания не найден");
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "CompanyController.GetUser");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        [HttpPost("SaveCompanyProfile/{id}")]
        public ActionResult SaveCompanyProfile(int id, [FromBody] Director newdirector)
        {
            try
            {
            if (!AuthContext.IsSelfOrAdmin(HttpContext, "company", id))
                return StatusCode(403, "Нет доступа");
            using (var context = new ProfnastavnikContext())
            {
                Director user = context.Directors.FirstOrDefault(x => x.IdDirector == id);
                if (user == null)
                {
                    return NotFound("Компания не найдена");
                }

                if (!string.IsNullOrEmpty(newdirector.NameCompanyDirector))
                    user.NameCompanyDirector = newdirector.NameCompanyDirector;

                if (!string.IsNullOrEmpty(newdirector.DescriptionDirector))
                    user.DescriptionDirector = newdirector.DescriptionDirector;

                if (!string.IsNullOrEmpty(newdirector.WebsiteDirector))
                    user.WebsiteDirector = newdirector.WebsiteDirector;

                if (!string.IsNullOrEmpty(newdirector.PhoneDirector))
                    user.PhoneDirector = newdirector.PhoneDirector;

                if (!string.IsNullOrEmpty(newdirector.CityDirector))
                    user.CityDirector = newdirector.CityDirector;

                if (!string.IsNullOrEmpty(newdirector.NameDirector))
                    user.NameDirector = newdirector.NameDirector;

                if (!string.IsNullOrEmpty(newdirector.SurnameDirector))
                    user.SurnameDirector = newdirector.SurnameDirector;

                if (!string.IsNullOrEmpty(newdirector.PatronymicDirector))
                    user.PatronymicDirector = newdirector.PatronymicDirector;


                context.SaveChanges();
                return Ok(user);
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "CompanyController.SaveCompanyProfile");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        [HttpGet("GetCompanyFeedback/{id}")]
        public ActionResult GetCompanyFeedback(int id)
        {
            try
            {
            using (var context = new ProfnastavnikContext())
            {
                List<FeedbacksUser> directory = context.FeedbacksUsers.Where(x => x.IdCompany == id && x.StatusFeedbackUser!="Удален").ToList();
               
                return directory.Count()>0 ? Ok(directory) : NotFound("");
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "CompanyController.GetCompanyFeedback");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        [HttpGet("GetCompanyFeedbackToUser/{id}")]
        public ActionResult GetCompanyFeedbackToUser(int id)
        {
            try
            {
            using (var context = new ProfnastavnikContext())
            {
                List<FeedbacksCompany> directory = context.FeedbacksCompanies.Where(x => x.IdCompany == id && x.StatusFeedbackCompany!="Удален").ToList();

                return directory.Count() > 0 ? Ok(directory) : NotFound("");
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "CompanyController.GetCompanyFeedbackToUser");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        [HttpDelete("DeleteGetCompanyFeedbackToUser/{id}")]
        public ActionResult DeleteGetCompanyFeedbackToUser(int id)
        {
            try
            {
            using (var context = new ProfnastavnikContext())
            {
                FeedbacksCompany directory = context.FeedbacksCompanies.FirstOrDefault(x => x.IdFeedbackCompany == id);
                if (directory == null)
                {
                    return NotFound("Отзыв не найден");
                }
                directory.StatusFeedbackCompany = "Удален";
                context.SaveChanges();
                return Ok(directory);
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "CompanyController.DeleteGetCompanyFeedbackToUser");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        [HttpGet("GetCompanyIntership/{id}")]
        public ActionResult GetCompanyIntership(int id)
        {
            try
            {
            using (var context = new ProfnastavnikContext())
            {
                List<Intership> directory = context.Interships.Where(x => x.IdCompany == id).ToList();
                if (directory.Count()==0)
                {
                    return NotFound("стажировки не найдены не найден");
                }
                return Ok(directory);
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "CompanyController.GetCompanyIntership");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        [HttpPost("CompanyFeddback")]
        public ActionResult CompanyFeddback(FeedbacksCompany f)
        {
            try
            {
            using (var context = new ProfnastavnikContext())
            {
                FeedbacksCompany old = context.FeedbacksCompanies.FirstOrDefault(x => x.IdCompany == f.IdCompany && x.IdUser == f.IdUser && x.IdVacancy == f.IdVacancy);
                if (old != null)
                {
                    return BadRequest("Вы уже оставляли этот отзыв");
                }

                f.IdFeedbackCompany = context.FeedbacksCompanies.Any() ? context.FeedbacksCompanies.Max(x => x.IdFeedbackCompany) + 1 : 1;
                f.StatusFeedbackCompany = "Новый";
                f.DateFeedback = DateOnly.FromDateTime(DateTime.Now);
                context.FeedbacksCompanies.Add(f);
                context.SaveChanges();
                return Ok(f);
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "CompanyController.CompanyFeddback");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        [HttpGet("GetCompanyMessages/{id}")]
        public ActionResult GetUserMessages(int id)
        {
            try
            {
            using (var context = new ProfnastavnikContext())
            {
                List<Chat> interships = context.Chats.Where(x => x.IdDirector == id && (x.SenderChat == "Стажер" || x.SenderChat=="Админ") && x.StatusChat != "Прочитано").ToList();

                return Ok(interships.Count);
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "CompanyController.GetUserMessages");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        [HttpGet("GetCompanyVacancies/{id}")]
        public ActionResult GetRecommendationsIntership(int id)
        {
            try
            {
            using (var context = new ProfnastavnikContext())
            {
                List<Vacancy> itog = context.Vacancies.Include(x=>x.CurrencyVacancyNavigation).Where(x => x.IdCompany == id && (x.StatusVacancy == "Активна" || x.StatusVacancy== "На модерации")).ToList();

                var list2 = itog.Select(x => new ListIntership
                {
                    idVacancy = x.IdVacancy,
                    NameVacancy = x.NameVacancy,
                    ZenStart = x.ZenStartVacancy,
                    Currency = x.CurrencyVacancyNavigation?.NameCurrency,
                    ZenEnd = x.ZenEndVacancy,
                    Time = x.TimeVacancy
                }).ToList();

                return list2.Count() > 0 ? Ok(list2) : NotFound("вакансии не найдены");
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "CompanyController.GetRecommendationsIntership");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        [HttpGet("GetCompanyResponcies/{id}")]
        public ActionResult GetCompanyResponcies(int id)
        {
            try
            {
            using (var context = new ProfnastavnikContext())
            {
                List<Response> itog = context.Responses.Include(x=>x.IdUserNavigation).Include(x=>x.IdVacancyNavigation).Where(x => x.IdCompany == id && x.StatusResponse == "Рассматривается").ToList();


                var list2 = itog.Select(x => new ResponciesDTO
                {
                    Description = x.DescriptionResponse,
                    NameVacancy = x.IdVacancyNavigation?.NameVacancy,
                    FIO = x.IdUserNavigation?.SurnameStudent+" "+ x.IdUserNavigation?.NameStudent+" "+ x.IdUserNavigation?.PatronymicStudent,
                    IdUser = x.IdUser,
                    IdVacancy = x.IdVacancy,
                   
                }).ToList();

                return list2.Count() > 0 ? Ok(list2) : NotFound("отклики не найдены");
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "CompanyController.GetCompanyResponcies");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        [HttpGet("GetCompanyChats/{id}")]
        public ActionResult GetUserChats(int id)
        {
            try
            {
            using (var context = new ProfnastavnikContext())
            {
                var chats = context.Chats.Include(x => x.IdVacancyNavigation).Include(x => x.IdDirectorNavigation).Include(x => x.IdUserNavigation)
                    .Where(x => x.IdDirector == id)
                    .OrderByDescending(x => x.SendAtChat)
                    .ToList();

                var itog = chats
                    .GroupBy(chat => chat.IdUser ?? (object)chat.EmailAdmin)
                    .Select(g => g.First()).Select(x => new ChatDTO
                    {
                        Chat = x,
                        NameCompany = x.IdDirectorNavigation?.NameCompanyDirector,
                        NameVacancy = x.IdVacancyNavigation?.NameVacancy,
                        FIOUser = x.IdUserNavigation?.SurnameStudent + " " + x.IdUserNavigation?.NameStudent + " " + x.IdUserNavigation?.PatronymicStudent
                    })
                    .ToList();

                return itog.Count > 0 ? Ok(itog) : NotFound("чаты не найдены");
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "CompanyController.GetUserChats");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        [HttpDelete("DeleteCompany/{id}")]
        public ActionResult DeleteCompany(int id)
        {
            try
            {
            if (!AuthContext.IsSelfOrAdmin(HttpContext, "company", id))
                return StatusCode(403, "Нет доступа");
            using (var context = new ProfnastavnikContext())
            {
                Director director = context.Directors.FirstOrDefault(x => x.IdDirector == id);
                if (director == null)
                {
                    return NotFound("компания не найдена");
                }
                director.StatusDirector = "Удален";
                Deleting d = new Deleting()
                {
                    IdDelete = context.Deletings.Any() ? context.Deletings.Max(x=>x.IdDelete) + 1 : 1,
                    EmailUser = director.EmailDirector,
                    DateDelete = DateOnly.FromDateTime(DateTime.Now)

                };
                List<Intership> inter = context.Interships.Where(x => x.IdCompany == director.IdDirector).ToList();
                foreach (var intership in inter)
                {
                    intership.StatusIntership = "Закончена";
                    intership.DateEndIntership = DateOnly.FromDateTime(DateTime.Now);

                }
                List<Response> resp = context.Responses.Where(x => x.IdCompany == director.IdDirector).ToList();
                foreach (var response in resp)
                {
                    response.StatusResponse = "Отклонено";
                }
                List<Vacancy> vac = context.Vacancies.Where(x => x.IdCompany == director.IdDirector).ToList();
                foreach (var vacancy in vac)
                {
                    vacancy.StatusVacancy = "Архив";
                }
                context.Deletings.Add(d);
                context.SaveChanges();
                return Ok(director);
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "CompanyController.DeleteCompany");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        [HttpGet("GetAllCompanyTarrifs")]
        public ActionResult GetAllCompanyTarrifs()
        {
            try
            {
            using (var context = new ProfnastavnikContext())
            {
                List<PricesForCompany> pr = context.PricesForCompanies.ToList();
                if (pr.Count() == 0)
                {
                    return NotFound("цены не найдены");
                }
               
                return Ok(pr);
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "CompanyController.GetAllCompanyTarrifs");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        [HttpGet("GetCompanyTarriff/{id}")]
        public ActionResult GetCompanyTarriff(int id)
        {
            try
            {
            using (var context = new ProfnastavnikContext())
            {
                CompanyPrice pr = context.CompanyPrices.FirstOrDefault(x=>x.IdCompany==id);
                if (pr == null)
                {
                    return NotFound("цены не найдены");
                }

                var tariff = TariffService.Compute(context, "company", id);
                return Ok(new CompanyPrice { Id = pr.Id, IdCompany = pr.IdCompany, IdPrice = tariff.TariffId });
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "CompanyController.GetCompanyTarriff");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        [HttpGet("GetTopStudents/{companyId}")]
        public ActionResult GetTopStudents(int companyId)
        {
            try
            {
            using (var context = new ProfnastavnikContext())
            {
                if (!AuthContext.IsSelfOrAdmin(HttpContext, "company", companyId))
                    return StatusCode(403, "Нет доступа");
                if (AuthContext.Get(HttpContext)?.Role != "admin")
                {
                    var tariff = TariffService.Compute(context, "company", companyId);
                    if (!tariff.TopStudents) return StatusCode(403, tariff.Locked("Доступ к топ 10% студентов"));
                }
                var companyVacancies = context.Vacancies
                    .Where(x => x.IdCompany == companyId && x.StatusVacancy == "Активна")
                    .ToList();

                if (!companyVacancies.Any())
                    return NotFound("Нет активных вакансий у компании");

                var allStudents = context.Students
                    .Where(s => s.StatusStudent == "Активен")
                    .ToList();

                var result = new List<TopStudentDTO>();

                foreach (var student in allStudents)
                {
                    var studentSkills = context.UserSkills
                        .Include(x => x.IdSkillNavigation)
                        .Where(x => x.IdUser == student.IdStudent)
                        .Select(x => x.IdSkillNavigation.NameSkill)
                        .ToList();

                    var studentProfessions = context.UserProfessions
                        .Include(x => x.IdProfessionNavigation)
                        .Where(x => x.IdUser == student.IdStudent)
                        .Select(x => x.IdProfessionNavigation.NameProfession)
                        .ToList();

                    var completedInternships = context.Interships
                        .Where(x => x.IdUser == student.IdStudent && x.StatusIntership == "Завершена")
                        .Count();

                    var studentRating = context.FeedbacksCompanies
                        .Where(x => x.IdUser == student.IdStudent && x.RatingStudent.HasValue&&x.StatusFeedbackCompany!="Удален")
                        .Average(x => x.RatingStudent) ?? 0;

                    var profileFields = new List<string>();
                    if (!string.IsNullOrEmpty(student.NameStudent)) profileFields.Add("name");
                    if (!string.IsNullOrEmpty(student.SurnameStudent)) profileFields.Add("surname");
                    if (!string.IsNullOrEmpty(student.PatronymicStudent)) profileFields.Add("patronymic");
                    if (!string.IsNullOrEmpty(student.FacultatyStudent)) profileFields.Add("faculty");
                    if (student.CourseStudent.HasValue) profileFields.Add("course");
                    if (student.UniversityStudent.HasValue) profileFields.Add("university");
                    if (student.BirthdayStudent.HasValue) profileFields.Add("birthday");

                    var profileCompleteness = (profileFields.Count / 7.0) * 100;

                    double bestCompatibility = 0;
                    foreach (var vacancy in companyVacancies)
                    {
                        var vacancySkills = context.VacancySkills
                            .Include(x => x.IdSkillNavigation)
                            .Where(x => x.IdVacancy == vacancy.IdVacancy)
                            .Select(x => x.IdSkillNavigation.NameSkill)
                            .ToList();

                        var vacancySferes = context.VacancySferes
                            .Include(x => x.IdSfereNavigation)
                            .Where(x => x.IdVacancy == vacancy.IdVacancy)
                            .Select(x => x.IdSfereNavigation.NameSfere)
                            .ToList();

                        var studentSferes = studentProfessions; 

                        var skillMatch = vacancySkills.Any() ?
                            (double)studentSkills.Count(s => vacancySkills.Contains(s)) / vacancySkills.Count * 100 : 0;

                        var sfereMatch = vacancySferes.Any() ?
                            (double)studentSferes.Count(s => vacancySferes.Contains(s)) / vacancySferes.Count * 100 : 0;

                        var compatibility = (skillMatch * 0.6) + (sfereMatch * 0.4);
                        if (compatibility > bestCompatibility)
                            bestCompatibility = compatibility;
                    }

                    var totalScore = (bestCompatibility * 0.25) +
                                   (Math.Min(completedInternships, 10) * 3) +
                                   (profileCompleteness * 0.15) +
                                   (studentRating * 5); 

                    result.Add(new TopStudentDTO
                    {
                        IdStudent = student.IdStudent,
                        FullName = $"{student.SurnameStudent} {student.NameStudent} {student.PatronymicStudent}".Trim(),
                        EmailStudent = student.EmailStudent,
                        CourseStudent = student.CourseStudent,
                        FacultatyStudent = student.FacultatyStudent,
                        UniversityStudent = context.Universities
                            .FirstOrDefault(u => u.IdUniversity == student.UniversityStudent)?.NameUniversity,
                        Skills = studentSkills,
                        Professions = studentProfessions,
                        CompletedInternships = completedInternships,
                        ProfileCompleteness = Math.Round(profileCompleteness, 1),
                        VacancyCompatibility = Math.Round(bestCompatibility, 1),
                        StudentRating = Math.Round(studentRating, 1),
                        TotalScore = Math.Round(totalScore, 1)
                    });
                }

                var topStudents = result
                    .OrderByDescending(x => x.TotalScore)
                    .Take(10)
                    .ToList();

                return topStudents.Any() ? Ok(topStudents) : NotFound("Студенты не найдены");
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "CompanyController.GetTopStudents");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        [PublicEndpoint]
        [HttpPost("CreateDirector")]
        public ActionResult CreateDirector([FromBody] Director director)
        {
            try
            {
            if (director == null || string.IsNullOrWhiteSpace(director.EmailDirector))
                return BadRequest("Не указан email");
            using (var context = new ProfnastavnikContext())
            {
                if (!context.Users.Any(u => u.EmailUser == director.EmailDirector && u.RoleUser == "Работодатель"))
                    return BadRequest("Сначала зарегистрируйтесь");
                if (context.Directors.Any(x => x.EmailDirector == director.EmailDirector))
                    return BadRequest("Профиль компании уже создан");
                director.IdDirector = context.Directors.Any() ? context.Directors.Max(x => x.IdDirector) + 1 : 1;
                director.StatusDirector = "На модерации";
                director.DateCreatedDirector = DateOnly.FromDateTime(DateTime.Now);
                director.TimeCreatedDirector = TimeOnly.FromDateTime(DateTime.Now);
                director.AuthenticationDirector = false;
                CompanyPrice cp = new CompanyPrice()
                {
                    IdCompany = director.IdDirector,
                    IdPrice = 1,
                    Id = context.CompanyPrices.Any() ? context.CompanyPrices.Max(x => x.Id) + 1 : 1,
                };
                context.CompanyPrices.Add(cp);
                context.Directors.Add(director);
                context.SaveChanges();

                return Ok(director);
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "CompanyController.CreateDirector");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }


        public class TopStudentDTO
        {
            public int IdStudent { get; set; }
            public string FullName { get; set; }
            public string EmailStudent { get; set; }
            public int? CourseStudent { get; set; }
            public string FacultatyStudent { get; set; }
            public string UniversityStudent { get; set; }
            public List<string> Skills { get; set; }
            public List<string> Professions { get; set; }
            public int CompletedInternships { get; set; }
            public double ProfileCompleteness { get; set; }
            public double VacancyCompatibility { get; set; }
            public double StudentRating { get; set; } 
            public double TotalScore { get; set; }
        }
        [HttpGet("GetCompanyResponsesCount/{companyId}")]
        public ActionResult GetCompanyResponsesCount(int companyId)
        {
            try
            {
            using (var context = new ProfnastavnikContext())
            {
                var date = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);
                var count = context.Responses
                    .Where(x => x.IdCompany == companyId && x.DateResponse >= date)
                    .Count();
                return Ok(count);
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "CompanyController.GetCompanyResponsesCount");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        [HttpPost("SearchStudents")]
        public ActionResult SearchStudents([FromBody] StudentSearchFilters filters)
        {
            try
            {
                var auth = AuthContext.Get(HttpContext);
                if (auth == null) return StatusCode(401, "Требуется авторизация");
                using (var context = new ProfnastavnikContext())
                {
                    if (auth.Role != "admin")
                    {
                        if (auth.Role != "company") return StatusCode(403, "Нет доступа");
                        var tariff = TariffService.Compute(context, "company", auth.UserId);
                        if (!tariff.SearchStudents) return StatusCode(403, tariff.Locked("Поиск стажёров"));
                    }

                    filters = filters ?? new StudentSearchFilters();
                    var query = context.Students
                        .Include(x => x.UniversityStudentNavigation)
                        .Where(x => x.StatusStudent == "Активен");

                    if (!string.IsNullOrWhiteSpace(filters.FullName))
                    {
                        foreach (var word in filters.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                        {
                            string w = word.ToLower();
                            query = query.Where(x =>
                                (x.NameStudent ?? "").ToLower().Contains(w) ||
                                (x.SurnameStudent ?? "").ToLower().Contains(w) ||
                                (x.PatronymicStudent ?? "").ToLower().Contains(w));
                        }
                    }

                    if (filters.MinCourse.HasValue) query = query.Where(x => x.CourseStudent >= filters.MinCourse);
                    if (filters.MaxCourse.HasValue) query = query.Where(x => x.CourseStudent <= filters.MaxCourse);

                    if (!string.IsNullOrWhiteSpace(filters.University))
                    {
                        string uni = filters.University.Trim().ToLower();
                        query = query.Where(x => x.UniversityStudentNavigation != null &&
                                                 (x.UniversityStudentNavigation.NameUniversity ?? "").ToLower() == uni);
                    }

                    var students = query.ToList();
                    var sids = students.Select(x => x.IdStudent).ToList();

                    var skillMap = context.UserSkills.Include(x => x.IdSkillNavigation)
                        .Where(x => x.IdUser != null && sids.Contains(x.IdUser.Value)).ToList()
                        .GroupBy(x => x.IdUser.Value)
                        .ToDictionary(g => g.Key, g => g.Select(x => x.IdSkillNavigation?.NameSkill).Where(v => v != null).Select(v => v!).ToList());
                    var profMap = context.UserProfessions.Include(x => x.IdProfessionNavigation)
                        .Where(x => x.IdUser != null && sids.Contains(x.IdUser.Value)).ToList()
                        .GroupBy(x => x.IdUser.Value)
                        .ToDictionary(g => g.Key, g => g.Select(x => x.IdProfessionNavigation?.NameProfession).Where(v => v != null).Select(v => v!).ToList());

                    var result = new List<StudentSearchResult>();
                    foreach (var student in students)
                    {
                        skillMap.TryGetValue(student.IdStudent, out var skills);
                        profMap.TryGetValue(student.IdStudent, out var professions);
                        skills = skills ?? new List<string>();
                        professions = professions ?? new List<string>();

                        if (filters.Skills != null && filters.Skills.Any() &&
                            !filters.Skills.All(sk => skills.Contains(sk, StringComparer.OrdinalIgnoreCase))) continue;

                        if (filters.Professions != null && filters.Professions.Any() &&
                            !filters.Professions.Any(p => professions.Contains(p, StringComparer.OrdinalIgnoreCase))) continue;

                        var fullName = $"{student.SurnameStudent} {student.NameStudent} {student.PatronymicStudent}".Trim();
                        result.Add(new StudentSearchResult
                        {
                            IdStudent = student.IdStudent,
                            FullName = fullName,
                            EmailStudent = student.EmailStudent,
                            CourseStudent = student.CourseStudent,
                            FacultatyStudent = student.FacultatyStudent,
                            UniversityStudent = student.UniversityStudentNavigation?.NameUniversity,
                            Skills = skills,
                            Professions = professions
                        });
                    }

                    result = result.OrderBy(x => x.FullName).ToList();
                    return result.Any() ? Ok(result) : NotFound("студенты не найдены");
                }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "CompanyController.SearchStudents");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        [HttpGet("GetPriorityResponses/{companyId}")]
        public ActionResult GetPriorityResponses(int companyId)
        {
            try
            {
            using (var context = new ProfnastavnikContext())
            {
                if (!AuthContext.IsSelfOrAdmin(HttpContext, "company", companyId))
                    return StatusCode(403, "Нет доступа");
                if (AuthContext.Get(HttpContext)?.Role != "admin")
                {
                    var tariff = TariffService.Compute(context, "company", companyId);
                    if (!tariff.PriorityResponses) return StatusCode(403, tariff.Locked("Приоритетные отклики студентов"));
                }
                var responses = context.Responses
                    .Include(x => x.IdUserNavigation)
                    .Include(x => x.IdVacancyNavigation)
                    .Where(x => x.IdCompany == companyId && x.StatusResponse == "Рассматривается")
                    .ToList();

                if (!responses.Any())
                    return NotFound("Нет откликов");

                var result = new List<PriorityResponseDTO>();

                foreach (var response in responses)
                {
                    var student = response.IdUserNavigation;
                    if (student == null) continue;

                    var studentSkills = context.UserSkills
                        .Include(x => x.IdSkillNavigation)
                        .Where(x => x.IdUser == student.IdStudent)
                        .Select(x => x.IdSkillNavigation.NameSkill)
                        .ToList();

                    var studentProfessions = context.UserProfessions
                        .Include(x => x.IdProfessionNavigation)
                        .Where(x => x.IdUser == student.IdStudent)
                        .Select(x => x.IdProfessionNavigation.NameProfession)
                        .ToList();

                    var vacancySkills = context.VacancySkills
                        .Include(x => x.IdSkillNavigation)
                        .Where(x => x.IdVacancy == response.IdVacancy)
                        .Select(x => x.IdSkillNavigation.NameSkill)
                        .ToList();

                    var vacancyProfessions = context.VacancySferes
                        .Include(x => x.IdSfereNavigation)
                        .Where(x => x.IdVacancy == response.IdVacancy)
                        .Select(x => x.IdSfereNavigation.NameSfere)
                        .ToList();

                    var completedInternships = context.Interships
                        .Where(x => x.IdUser == student.IdStudent && x.StatusIntership == "Завершена")
                        .Count();

                    var studentRating = context.FeedbacksCompanies
                        .Where(x => x.IdUser == student.IdStudent && x.RatingStudent.HasValue&&x.StatusFeedbackCompany!="Удален")
                        .Average(x => x.RatingStudent) ?? 0;

                    var skillMatchCount = studentSkills.Count(s => vacancySkills.Contains(s));
                    var skillMatchPercent = vacancySkills.Any() ? (double)skillMatchCount / vacancySkills.Count * 100 : 0;

                    var professionMatchCount = studentProfessions.Count(p => vacancyProfessions.Contains(p));
                    var professionMatchPercent = vacancyProfessions.Any() ? (double)professionMatchCount / vacancyProfessions.Count * 100 : 0;

                    var totalScore = (skillMatchPercent * 0.25) +      
                                   (professionMatchPercent * 0.20) +  
                                   (Math.Min(completedInternships, 5) * 4) + 
                                   (studentRating * 5) +             
                                   (string.IsNullOrEmpty(response.PortfolioResponse) ? 0 : 10); 

                    result.Add(new PriorityResponseDTO
                    {
                        Response = response,
                        StudentFIO = $"{student.SurnameStudent} {student.NameStudent} {student.PatronymicStudent}".Trim(),
                        VacancyName = response.IdVacancyNavigation?.NameVacancy,
                        SkillMatchCount = skillMatchCount,
                        SkillMatchPercent = Math.Round(skillMatchPercent, 1),
                        ProfessionMatchCount = professionMatchCount,
                        ProfessionMatchPercent = Math.Round(professionMatchPercent, 1),
                        CompletedInternships = completedInternships,
                        StudentRating = Math.Round(studentRating, 1),
                        TotalScore = Math.Round(totalScore, 1),
                        HasPortfolio = !string.IsNullOrEmpty(response.PortfolioResponse)
                    });
                }

                result = result.OrderByDescending(x => x.TotalScore).ToList();

                return Ok(result);
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "CompanyController.GetPriorityResponses");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }

        public class PriorityResponseDTO
        {
            public Response Response { get; set; }
            public string StudentFIO { get; set; }
            public string VacancyName { get; set; }
            public int SkillMatchCount { get; set; }
            public double SkillMatchPercent { get; set; }
            public int ProfessionMatchCount { get; set; }
            public double ProfessionMatchPercent { get; set; }
            public int CompletedInternships { get; set; }
            public double StudentRating { get; set; } 
            public double TotalScore { get; set; }
            public bool HasPortfolio { get; set; }
        }
        public class StudentSearchFilters
        {
            public string? FullName { get; set; }
            public List<string>? Skills { get; set; }
            public List<string>? Professions { get; set; }
            public string? University { get; set; }
            public int? MinCourse { get; set; }
            public int? MaxCourse { get; set; }
        }

        public class StudentSearchResult
        {
            public int IdStudent { get; set; }
            public string? FullName { get; set; }
            public string? EmailStudent { get; set; }
            public int? CourseStudent { get; set; }
            public string? FacultatyStudent { get; set; }
            public string? UniversityStudent { get; set; }
            public List<string>? Skills { get; set; }
            public List<string>? Professions { get; set; }
        }
        [HttpPost("AddNewCompanyPrice")]
        public ActionResult AddNewCompanyPrice(CompanyPrice s)
        {
            try
            {
            using (var context = new ProfnastavnikContext())
            {
                if (s == null || !AuthContext.IsSelfOrAdmin(HttpContext, "company", s.IdCompany ?? 0))
                    return StatusCode(403, "Нет доступа");
                CompanyPrice current = context.CompanyPrices.FirstOrDefault(x => x.IdCompany == s.IdCompany);
                if (current == null) return NotFound("Тариф компании не найден");
                current.IdPrice = s.IdPrice;
                CompanyPay cp = new CompanyPay()
                {
                    IdCompanyPay = context.CompanyPays.Any() ? context.CompanyPays.Max(x => x.IdCompanyPay) + 1 : 1,
                    IdCompany = current.IdCompany,
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    IdPay = s.IdPrice

                };
               
                context.CompanyPays.Add(cp);
                context.SaveChanges();
                return Ok(current);
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "CompanyController.AddNewCompanyPrice");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }

    }
}
