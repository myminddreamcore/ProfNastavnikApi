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
    [ApiController]
    [TokenAuth]
    public class ResponcesController : ControllerBase
    {


        // GET api/<IntershipsController>/5
        [HttpGet("GetUserConsiderIntership/{id}")]
        public ActionResult GetUserConsiderIntership(int id)
        {
            try
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
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "ResponcesController.GetUserConsiderIntership");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        // GET api/<IntershipsController>/5
        [HttpPost("AddResponse")]
        public ActionResult AddResponse(Response s)
        {
            try
            {
            using (var context = new ProfnastavnikContext())
            {
                Response current = context.Responses.FirstOrDefault(x => x.IdCompany == s.IdCompany && x.IdUser == s.IdUser && x.IdVacancy==s.IdVacancy);
                if (current != null)
                {
                    return BadRequest("вы уже откликались на эту вакансию");
                }
                // Лимит откликов в месяц по тарифу («Старт»: 5, «Профи»: без ограничений)
                if (s.IdUser == null) return BadRequest("Не указан пользователь");
                if (!AuthContext.IsSelfOrAdmin(HttpContext, "student", s.IdUser.Value))
                    return StatusCode(403, "Нет доступа");
                if (AuthContext.Get(HttpContext)?.Role != "admin")
                {
                    var tariff = TariffService.Compute(context, "student", s.IdUser.Value);
                    if (tariff.MaxResponsesPerMonth >= 0 && tariff.ResponsesUsed >= tariff.MaxResponsesPerMonth)
                        return StatusCode(403, tariff.LimitReached(tariff.MaxResponsesPerMonth + " откликов в месяц"));
                }
                s.IdResponse = context.Responses.Any() ? context.Responses.Max(x => x.IdResponse) + 1 : 1;
                s.StatusResponse = "Рассматривается";
                s.DateResponse = DateOnly.FromDateTime(DateTime.Now);
                context.Responses.Add(s);
                context.SaveChanges();
                return s!=null ? Ok(s) : NotFound("рассматриваемые вакансии не найдены");
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "ResponcesController.AddResponse");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        [HttpPost("AddNewUserPrice")]
        public ActionResult AddNewUserPrice(UserPrice s)
        {
            try
            {
            using (var context = new ProfnastavnikContext())
            {
                if (s == null || !AuthContext.IsSelfOrAdmin(HttpContext, "student", s.IdUser ?? 0))
                    return StatusCode(403, "Нет доступа");
                UserPrice current = context.UserPrices.FirstOrDefault(x => x.IdUser == s.IdUser);
                if (current == null) return NotFound("Тариф пользователя не найден");
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
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "ResponcesController.AddNewUserPrice");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        // GET api/<IntershipsController>/5
        [HttpPost("GetFilterIntership")]
        public ActionResult GetFilterIntership(Filters filter)
        {
            try
            {
                using (var context = new ProfnastavnikContext())
                {
                    // Кто ищет, берём из токена (студент не может искать «от имени» другого)
                    var auth = AuthContext.Get(HttpContext);
                    if (auth != null && auth.Role == "student") filter.iduser = auth.UserId;

                    var query = context.Vacancies
                        .Include(x => x.CurrencyVacancyNavigation)
                        .Include(x => x.Adresses)
                        .Where(x => x.StatusVacancy == "Активна");

                    // Поиск по названию И описанию, без учёта регистра
                    if (!string.IsNullOrWhiteSpace(filter.name))
                    {
                        string n = filter.name.Trim().ToLower();
                        query = query.Where(x => (x.NameVacancy ?? "").ToLower().Contains(n)
                                              || (x.DescroptionVacancy ?? "").ToLower().Contains(n));
                    }

                    if (filter.user.HasValue)
                    {
                        query = query.Where(x => x.AnybodyVacancy == filter.user.Value);
                    }

                    // Зарплата: диапазон вакансии должен ПЕРЕСЕКАТЬСЯ с желаемым диапазоном.
                    // Раньше требовалось, чтобы вакансия целиком лежала внутри диапазона, и многие подходящие пропадали;
                    // вакансии без верхней границы тоже терялись.
                    if (filter.zenstart.HasValue)
                    {
                        var min = filter.zenstart.Value;
                        query = query.Where(x => (x.ZenEndVacancy ?? x.ZenStartVacancy ?? 0) >= min);
                    }
                    if (filter.zenEnd.HasValue)
                    {
                        var max = filter.zenEnd.Value;
                        query = query.Where(x => (x.ZenStartVacancy ?? 0) <= max);
                    }

                    if (!string.IsNullOrWhiteSpace(filter.city))
                    {
                        string city = filter.city.Trim().ToLower();
                        query = query.Where(x => x.Adresses.Any(a => (a.CityAdress ?? "").ToLower().Contains(city)));
                    }

                    var list = query.ToList();
                    var ids = list.Select(x => x.IdVacancy).ToList();

                    // Вакансии, на которые студент уже откликнулся, не показываем (один запрос вместо запроса на каждую вакансию)
                    var already = context.Responses
                        .Where(x => x.IdUser == filter.iduser && x.IdVacancy != null && ids.Contains(x.IdVacancy.Value))
                        .Select(x => x.IdVacancy.Value).ToList().ToHashSet();

                    // Справочники по всем найденным вакансиям загружаем сразу, без запросов в цикле
                    var formatMap = context.VacanciesFormats.Include(x => x.IdFormatNavigation)
                        .Where(x => x.IdVacancy != null && ids.Contains(x.IdVacancy.Value)).ToList()
                        .GroupBy(x => x.IdVacancy.Value)
                        .ToDictionary(g => g.Key, g => g.Select(x => x.IdFormatNavigation?.NameVacancyFormat).Where(v => v != null).Select(v => v!).ToList());
                    var sfereMap = context.VacancySferes.Include(x => x.IdSfereNavigation)
                        .Where(x => x.IdVacancy != null && ids.Contains(x.IdVacancy.Value)).ToList()
                        .GroupBy(x => x.IdVacancy.Value)
                        .ToDictionary(g => g.Key, g => g.Select(x => x.IdSfereNavigation?.NameSfere).Where(v => v != null).Select(v => v!).ToList());
                    var skillMap = context.VacancySkills.Include(x => x.IdSkillNavigation)
                        .Where(x => x.IdVacancy != null && ids.Contains(x.IdVacancy.Value)).ToList()
                        .GroupBy(x => x.IdVacancy.Value)
                        .ToDictionary(g => g.Key, g => g.Select(x => x.IdSkillNavigation?.NameSkill).Where(v => v != null).Select(v => v!).ToList());

                    var empty = new List<string>();
                    var itog = new List<Vacancy>();
                    foreach (var item in list)
                    {
                        if (already.Contains(item.IdVacancy)) continue;

                        bool formatsOk = true, sferesOk = true, skillsOk = true;
                        if (filter.formats != null && filter.formats.Any())
                        {
                            formatMap.TryGetValue(item.IdVacancy, out var vf);
                            formatsOk = (vf ?? empty).Intersect(filter.formats, StringComparer.OrdinalIgnoreCase).Any();
                        }
                        if (filter.sferes != null && filter.sferes.Any())
                        {
                            sfereMap.TryGetValue(item.IdVacancy, out var vs);
                            sferesOk = (vs ?? empty).Intersect(filter.sferes, StringComparer.OrdinalIgnoreCase).Any();
                        }
                        if (filter.skills != null && filter.skills.Any())
                        {
                            skillMap.TryGetValue(item.IdVacancy, out var vk);
                            skillsOk = (vk ?? empty).Intersect(filter.skills, StringComparer.OrdinalIgnoreCase).Any();
                        }
                        if (skillsOk && sferesOk && formatsOk) itog.Add(item);
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
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "ResponcesController.GetFilterIntership");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        [HttpGet("GetRecommendationsIntership/{id}")]
        public ActionResult GetRecommendationsIntership(int id)
        {
            try
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
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "ResponcesController.GetRecommendationsIntership");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        // GET api/<IntershipsController>/5
        [HttpGet("GetUserInvitedIntership/{id}")]
        public ActionResult GetUserInvitedIntership(int id)
        {
            try
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
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "ResponcesController.GetUserInvitedIntership");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        // GET api/<IntershipsController>/5
        [HttpGet("GetUserRejectedIntership/{id}")]
        public ActionResult GetUserRejectedIntership(int id)
        {
            try
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
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "ResponcesController.GetUserRejectedIntership");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        [HttpGet("GetResponsesByCompany/{id}")]
        public ActionResult GetResponsesByCompany(int id)
        {
            try
            {
            using (var context = new ProfnastavnikContext())
            {
                List<Response> list = context.Responses.Where(x => x.IdCompany == id && x.StatusResponse == "Рассматривается").OrderByDescending(x => x.IdResponse).Include(x => x.IdVacancyNavigation).Include(x => x.IdVacancyNavigation.CurrencyVacancyNavigation).ToList();
                
                return list.Count() > 0 ? Ok(list) : NotFound("новых откликов нет");
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "ResponcesController.GetResponsesByCompany");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        [HttpPut("UpdateResponseStatus/{id}/{status}")]
        public ActionResult UpdateResponseStatus(int id, string status)
        {
            try
            {
            using (var context = new ProfnastavnikContext())
            {
                Response r = context.Responses.FirstOrDefault(x => x.IdResponse == id);
                if (r == null) return NotFound("Отклик не найден");
                if (!AuthContext.IsSelfOrAdmin(HttpContext, "company", r.IdCompany ?? 0))
                    return StatusCode(403, "Нет доступа");
                if (AuthContext.Get(HttpContext)?.Role != "admin" && r.IdCompany != null)
                {
                    // Обрабатывать можно только первые N откликов месяца (Базовый: 5, Бизнес: 20, Корпоративный: все)
                    var tariff = TariffService.Compute(context, "company", r.IdCompany.Value);
                    if (tariff.MaxResponsesPerMonth >= 0)
                    {
                        var monthStart = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);
                        var ids = context.Responses
                            .Where(x => x.IdCompany == r.IdCompany && x.DateResponse >= monthStart)
                            .OrderBy(x => x.IdResponse).Select(x => x.IdResponse).ToList();
                        int rank = ids.IndexOf(r.IdResponse) + 1;
                        if (rank > tariff.MaxResponsesPerMonth)
                            return StatusCode(403, tariff.LimitReached(tariff.MaxResponsesPerMonth + " откликов в месяц"));
                    }
                }
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
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "ResponcesController.UpdateResponseStatus");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }


    }
}
