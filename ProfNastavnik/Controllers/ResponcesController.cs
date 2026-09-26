using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfNastavnik.DTO;
using ProfNastavnik.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ProfNastavnik.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResponcesController : ControllerBase
    {


        // GET api/<IntershipsController>/5
        [HttpGet("GetUserConsiderIntership/{id}")]
        public ActionResult GetUserConsiderIntership(int id)
        {
            using (var context = new ProfnastavnikContext())
            {
                List<Response> list = context.Responses.Where(x => x.IdUser == id && x.StatusResponse == "Рассматривается").OrderByDescending(x => x.IdResponse).Include(x => x.IdVacancyNavigation).Include(x => x.IdVacancyNavigation.CurrencyVacancyNavigation).ToList();
                var list2 = list.Select(x => new ListIntership
                {
                    idVacancy = x.IdVacancy,
                    NameVacancy = x.IdVacancyNavigation?.NameVacancy,
                    ZenStart = x.IdVacancyNavigation?.ZenStartVacancy,
                    Currency = x.IdVacancyNavigation?.CurrencyVacancyNavigation?.NameCurrency,
                    ZenEnd = x.IdVacancyNavigation?.ZenEndVacancy,
                    Time = x.IdVacancyNavigation?.TimeVacancy

                }).ToList();
                return list2.Count()>0 ? Ok(list2 ) : NotFound("рассматриваемые вакансии не найдены");
            }
        }
        // GET api/<IntershipsController>/5
        [HttpPost("AddResponse")]
        public ActionResult AddResponse(Response s)
        {
            using (var context = new ProfnastavnikContext())
            {
                Response current = context.Responses.FirstOrDefault(x => x.IdCompany == s.IdCompany && x.IdUser == s.IdUser && x.IdVacancy==s.IdVacancy);
                if (current != null)
                {
                    return BadRequest("вы уже откликались на эту вакансию");
                }
                s.IdResponse = context.Responses.Any() ? context.Responses.Max(x => x.IdResponse) + 1 : 1;
                s.StatusResponse = "Рассматривается";
                s.DateResponse = DateOnly.FromDateTime(DateTime.Now);
                context.Responses.Add(s);
                context.SaveChanges();
                return s!=null ? Ok(s) : NotFound("рассматриваемые вакансии не найдены");
            }
        }
        [HttpPost("AddNewUserPrice")]
        public ActionResult AddNewUserPrice(UserPrice s)
        {
            using (var context = new ProfnastavnikContext())
            {
                UserPrice current = context.UserPrices.FirstOrDefault(x => x.IdUser == s.IdUser);
                current.IdPrices = s.IdPrices;
                UserPay pay = new UserPay()
                {
                    IdUserPay = context.UserPays.Any() ? context.UserPays.Max(x => x.IdUserPay) + 1 : 1,
                    IdPrice = s.IdPrices,
                    IdUser = s.IdUser,
                    Date = DateOnly.FromDateTime(DateTime.Now)
                };
                context.UserPays.Add(pay);
                context.SaveChanges();
                return s != null ? Ok(s) : NotFound("рассматриваемые вакансии не найдены");
            }
        }
        // GET api/<IntershipsController>/5
        [HttpPost("GetFilterIntership")]
        public ActionResult GetFilterIntership(Filters filter)
        {
            using (var context = new ProfnastavnikContext())
            {
                var query = context.Vacancies
                    .Include(x => x.CurrencyVacancyNavigation)
                    .Include(x => x.Adresses)
                    .Where(x => x.StatusVacancy == "Активна");

                if (!string.IsNullOrEmpty(filter.name))
                {
                    query = query.Where(x => x.NameVacancy.ToLower().Contains(filter.name.ToLower()));
                }

                if (filter.user.HasValue)
                {
                    query = query.Where(x => x.AnybodyVacancy == filter.user.Value);
                }

                if (filter.zenstart.HasValue)
                {
                    query = query.Where(x => x.ZenStartVacancy >= filter.zenstart.Value);
                }
                if (filter.zenEnd.HasValue)
                {
                    query = query.Where(x => x.ZenEndVacancy <= filter.zenEnd.Value);
                }

                if (!string.IsNullOrEmpty(filter.city))
                {
                    query = query.Where(x => x.Adresses.Any(a =>
                        a.CityAdress.ToLower().Contains(filter.city.ToLower())));
                }

                var list = query.ToList();
                List<Vacancy> itog = new List<Vacancy>();

                foreach (var item in list)
                {
                    bool skillsOk = true;
                    bool sferesOk = true;
                    bool formatsOk = true;

                    Response r = context.Responses
                        .FirstOrDefault(x => x.IdVacancy == item.IdVacancy && x.IdUser == filter.iduser);

                    if (r != null)
                    {
                        continue; 
                    }

                    if (filter.formats != null && filter.formats.Any())
                    {
                        var vacancyFormats = context.VacanciesFormats
                            .Include(x => x.IdFormatNavigation)
                            .Where(x => x.IdVacancy == item.IdVacancy)
                            .Select(x => x.IdFormatNavigation.NameVacancyFormat)
                            .ToList();

                        formatsOk = vacancyFormats.Intersect(filter.formats).Any();
                    }
                    if (filter.sferes != null && filter.sferes.Any())
                    {
                        var vacancySferes = context.VacancySferes
                            .Include(x => x.IdSfereNavigation)
                            .Where(x => x.IdVacancy == item.IdVacancy)
                            .Select(x => x.IdSfereNavigation.NameSfere)
                            .ToList();

                        sferesOk = vacancySferes.Intersect(filter.sferes).Any();
                    }

                    if (filter.skills != null && filter.skills.Any())
                    {
                        var vacancySkills = context.VacancySkills
                            .Include(x => x.IdSkillNavigation)
                            .Where(x => x.IdVacancy == item.IdVacancy)
                            .Select(x => x.IdSkillNavigation.NameSkill)
                            .ToList();

                        skillsOk = vacancySkills.Intersect(filter.skills).Any();
                    }

                    if (skillsOk && sferesOk && formatsOk)
                    {
                        itog.Add(item);
                    }
                }

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
        [HttpGet("GetRecommendationsIntership/{id}")]
        public ActionResult GetRecommendationsIntership(int id)
        {
            using (var context = new ProfnastavnikContext())
            {
                List<string> userProfessions = context.UserProfessions
                    .Include(x => x.IdProfessionNavigation)
                    .Where(x => x.IdUser == id)
                    .Select(x => x.IdProfessionNavigation.NameProfession)
                    .ToList();

                List<string> userSkills = context.UserSkills
                    .Include(x => x.IdSkillNavigation)
                    .Where(x => x.IdUser == id)
                    .Select(x => x.IdSkillNavigation.NameSkill)
                    .ToList();

                var vacancies = context.Vacancies
                    .Include(x => x.CurrencyVacancyNavigation)
                    .Where(x => x.StatusVacancy == "Активна")
                    .ToList();

                List<Vacancy> itog = new List<Vacancy>();

                foreach (var vacancy in vacancies)
                {
                    Response r = context.Responses
                        .FirstOrDefault(x => x.IdVacancy == vacancy.IdVacancy && x.IdUser == id);

                    if (r != null)
                    {
                        continue;
                    }

                    bool isMatch = false;

                    List<string> vacancySferes = context.VacancySferes
                        .Include(x => x.IdSfereNavigation)
                        .Where(x => x.IdVacancy == vacancy.IdVacancy)
                        .Select(x => x.IdSfereNavigation.NameSfere)
                        .ToList();

                    if (userProfessions.Any(p => vacancySferes.Contains(p)))
                    {
                        isMatch = true;
                    }

                    if (!isMatch)
                    {
                        List<string> vacancySkills = context.VacancySkills
                            .Include(x => x.IdSkillNavigation)
                            .Where(x => x.IdVacancy == vacancy.IdVacancy)
                            .Select(x => x.IdSkillNavigation.NameSkill)
                            .ToList();

                        if (userSkills.Any(s => vacancySkills.Contains(s)))
                        {
                            isMatch = true;
                        }
                    }

                    if (isMatch && !itog.Contains(vacancy))
                    {
                        itog.Add(vacancy);
                    }
                }

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
        // GET api/<IntershipsController>/5
        [HttpGet("GetUserInvitedIntership/{id}")]
        public ActionResult GetUserInvitedIntership(int id)
        {
            using (var context = new ProfnastavnikContext())
            {
                List<Response> list = context.Responses.Where(x => x.IdUser == id && x.StatusResponse == "Приглашен").OrderByDescending(x => x.IdResponse).Include(x => x.IdVacancyNavigation).Include(x => x.IdVacancyNavigation.CurrencyVacancyNavigation).ToList();
                var list2 = list.Select(x => new ListIntership
                {
                    idVacancy = x.IdVacancy,
                    NameVacancy = x.IdVacancyNavigation?.NameVacancy,
                    ZenStart = x.IdVacancyNavigation?.ZenStartVacancy,
                    Currency = x.IdVacancyNavigation?.CurrencyVacancyNavigation?.NameCurrency,
                    ZenEnd = x.IdVacancyNavigation?.ZenEndVacancy,
                    Time = x.IdVacancyNavigation?.TimeVacancy

                }).ToList();
                return list2.Count() > 0 ? Ok(list2) : NotFound("рассматриваемые вакансии не найдены");
            }
        }
        // GET api/<IntershipsController>/5
        [HttpGet("GetUserRejectedIntership/{id}")]
        public ActionResult GetUserRejectedIntership(int id)
        {
            using (var context = new ProfnastavnikContext())
            {
                List<Response> list = context.Responses.Where(x => x.IdUser == id && x.StatusResponse == "Отклонено").OrderByDescending(x => x.IdResponse).Include(x => x.IdVacancyNavigation).Include(x=>x.IdVacancyNavigation.CurrencyVacancyNavigation).ToList();
                var list2 = list.Select(x => new ListIntership
                {
                    idVacancy = x.IdVacancy,
                    NameVacancy = x.IdVacancyNavigation?.NameVacancy,
                    ZenStart = x.IdVacancyNavigation?.ZenStartVacancy,
                    Currency = x.IdVacancyNavigation?.CurrencyVacancyNavigation?.NameCurrency,
                    ZenEnd = x.IdVacancyNavigation?.ZenEndVacancy,
                    Time = x.IdVacancyNavigation?.TimeVacancy

                }).ToList();
                return list2.Count() > 0 ? Ok(list2) : NotFound("рассматриваемые вакансии не найдены");
            }
        }
        [HttpGet("GetResponsesByCompany/{id}")]
        public ActionResult GetResponsesByCompany(int id)
        {
            using (var context = new ProfnastavnikContext())
            {
                List<Response> list = context.Responses.Where(x => x.IdCompany == id && x.StatusResponse == "Рассматривается").OrderByDescending(x => x.IdResponse).Include(x => x.IdVacancyNavigation).Include(x => x.IdVacancyNavigation.CurrencyVacancyNavigation).ToList();
                
                return list.Count() > 0 ? Ok(list) : NotFound("новых откликов нет");
            }
        }
        [HttpPut("UpdateResponseStatus/{id}/{status}")]
        public ActionResult UpdateResponseStatus(int id, string status)
        {
            using (var context = new ProfnastavnikContext())
            {
                Response r = context.Responses.FirstOrDefault(x => x.IdResponse == id);
                r.StatusResponse = status;
                if (status == "Приглашен")
                {
                    Chat t = new Chat()
                    {
                        IdUser = r.IdUser,
                        IdChat = context.Chats.Any() ? context.Chats.Max(x => x.IdChat) + 1 : 1,
                        IdDirector = r.IdCompany,
                        IdVacancy = r.IdVacancy,
                        TextChat = "Здравствуйте! Ваш отклик был принят и мы готовы вас пригласить!",
                        StatusChat = "Отправлено",
                        SenderChat = "Работодатель",
                        SendAtChat = DateTime.Now,
                    };
                    context.Chats.Add(t);
                }
                context.SaveChanges();
                return Ok(r);
            }
        }


    }
}
