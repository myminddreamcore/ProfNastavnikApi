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
    public class VacancyController : ControllerBase
    {
       

        // GET api/<VacancyController>/5
        [HttpGet("GetVacancy/{id}")]
        public ActionResult Get(int id)
        {
            try
            {
            using(var context = new ProfnastavnikContext())
            {
                Vacancy vacancy = context.Vacancies.Include(x=>x.CurrencyVacancyNavigation).Include(x=>x.IdCompanyNavigation).FirstOrDefault(x=>x.IdVacancy == id);

                List<string> l = new List<string>();
                var t = context.VacanciesFormats.Include(x=>x.IdFormatNavigation).Where(x=>x.IdVacancy==id);
                foreach (var format in t)
                {
                    l.Add(format.IdFormatNavigation.NameVacancyFormat);
                }

                List<string> sferes = new List<string>();
                var m = context.VacancySferes.Include(x => x.IdSfereNavigation).Where(x => x.IdVacancy == id);
                foreach (var format in m)
                {
                    sferes.Add(format.IdSfereNavigation.NameSfere);
                }

                List<string> skills = new List<string>();
                var q = context.VacancySkills.Include(x => x.IdSkillNavigation).Where(x => x.IdVacancy == id);
                foreach (var format in q)
                {
                    skills.Add(format.IdSkillNavigation.NameSkill);
                }
                Adress a = context.Adresses.FirstOrDefault(x => x.IdVacancy == vacancy.IdVacancy);

                CardVacancy cv = new CardVacancy()
                {
                    Vacancy = vacancy,
                    Formats = l,
                    Currency = vacancy.CurrencyVacancyNavigation.NameCurrency,
                    NameCompany = vacancy.IdCompanyNavigation?.NameCompanyDirector,
                    CompanyId = vacancy.IdCompany,
                    Skills = skills,
                    Sferes = sferes,
                    Adress = "Город " + a?.CityAdress + ", Улица " + a?.StreetAdress + " " + a?.HouseAdress + ", " + a?.FlatAdress + ", Индекс " + a?.IndexAdress,
                    RatingCompany = context.FeedbacksUsers.Where(x => x.IdCompany == vacancy.IdCompany)?.Count() > 0 ? context.FeedbacksUsers.Where(x => x.IdCompany == vacancy.IdCompany)?.Sum(x => x.RatingFeedbackUser) / context.FeedbacksUsers.Where(x => x.IdCompany == vacancy.IdCompany)?.Count() : 0,
                };
                return vacancy != null ? Ok(cv) : NotFound("вакансия не найдена");
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "VacancyController.Get");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        [HttpPost("CreateVacancy")]
        public ActionResult CreateVacancy([FromBody] CreateVacancyDTO dto)
        {
            try
            {
            using (var context = new ProfnastavnikContext())
            {
                if (dto?.Vacancy == null || dto.Adress == null)
                    return BadRequest("Некорректные данные вакансии");
                if (!AuthContext.IsSelfOrAdmin(HttpContext, "company", dto.Vacancy.IdCompany ?? 0))
                    return StatusCode(403, "Нет доступа");
                // Лимит вакансий по тарифу (Базовый: 1, Бизнес: 5, Корпоративный: без ограничений)
                if (AuthContext.Get(HttpContext)?.Role != "admin")
                {
                    var tariff = TariffService.Compute(context, "company", dto.Vacancy.IdCompany ?? 0);
                    if (tariff.MaxVacancies >= 0 && tariff.VacanciesUsed >= tariff.MaxVacancies)
                        return StatusCode(403, tariff.LimitReached(tariff.MaxVacancies + " вакансий"));
                }

                var vacancy = context.Vacancies.FirstOrDefault(x =>
                    x.NameVacancy == dto.Vacancy.NameVacancy &&
                    x.IdCompany == dto.Vacancy.IdCompany);

                if (vacancy != null)
                {
                    return BadRequest("такая вакансия у вас уже есть");
                }

                dto.Vacancy.IdVacancy = context.Vacancies.Any() ? context.Vacancies.Max(x => x.IdVacancy) + 1 : 1;
                dto.Vacancy.StatusVacancy = "На модерации";
                dto.Vacancy.DateCreated = DateOnly.FromDateTime(DateTime.Now);

                dto.Adress.IdAdress = context.Adresses.Any() ? context.Adresses.Max(x => x.IdAdress) + 1 : 1;
                dto.Adress.IdVacancy = dto.Vacancy.IdVacancy;

                context.Vacancies.Add(dto.Vacancy);
                context.Adresses.Add(dto.Adress);

                if (dto.Skills != null && dto.Skills.Any())
                {
                    foreach (var skillName in dto.Skills)
                    {
                        var skill = context.Skills.FirstOrDefault(s => s.NameSkill == skillName);
                        if (skill == null)
                        {
                            skill = new Skill
                            {
                                IdSkill = context.Skills.Any() ? context.Skills.Max(s => s.IdSkill) + 1 : 1,
                                NameSkill = skillName
                            };
                            context.Skills.Add(skill);
                            context.SaveChanges();
                        }

                        var newId = context.VacancySkills.Any()
                            ? context.VacancySkills.Max(vs => vs.IdVacationSkill) + 1
                            : 1;

                        context.VacancySkills.Add(new VacancySkill
                        {
                            IdVacationSkill = newId,
                            IdVacancy = dto.Vacancy.IdVacancy,
                            IdSkill = skill.IdSkill
                        });
                    }
                }

                if (dto.Sferes != null && dto.Sferes.Any())
                {
                    foreach (var sfereName in dto.Sferes)
                    {
                        var sfere = context.Sferes.FirstOrDefault(s => s.NameSfere == sfereName);
                        if (sfere == null)
                        {
                            sfere = new Sfere
                            {
                                IdSfere = context.Sferes.Any() ? context.Sferes.Max(s => s.IdSfere) + 1 : 1,
                                NameSfere = sfereName
                            };
                            context.Sferes.Add(sfere);
                            context.SaveChanges();
                        }

                        var newId = context.VacancySferes.Any()
                            ? context.VacancySferes.Max(vs => vs.IdVacationSfere) + 1
                            : 1;

                        context.VacancySferes.Add(new VacancySfere
                        {
                            IdVacationSfere = newId,
                            IdVacancy = dto.Vacancy.IdVacancy,
                            IdSfere = sfere.IdSfere
                        });
                    }
                }

                

                if (!string.IsNullOrEmpty(dto.Format))
                {
                    var format = context.VacancyFormats.FirstOrDefault(f => f.NameVacancyFormat == dto.Format);
                    if (format == null)
                    {
                        format = new VacancyFormat
                        {
                            IdVacancyFormat = context.VacancyFormats.Any()
                                ? context.VacancyFormats.Max(f => f.IdVacancyFormat) + 1
                                : 1,
                            NameVacancyFormat = dto.Format
                        };
                        context.VacancyFormats.Add(format);
                        context.SaveChanges();
                    }

                    var newId = context.VacanciesFormats.Any()
                        ? context.VacanciesFormats.Max(vf => vf.IdVacancyFormat) + 1
                        : 1;

                    context.VacanciesFormats.Add(new VacanciesFormat
                    {
                        IdVacancyFormat = newId,
                        IdVacancy = dto.Vacancy.IdVacancy,
                        IdFormat = format.IdVacancyFormat
                    });
                }

                context.SaveChanges();

                return Ok(dto.Vacancy);
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "VacancyController.CreateVacancy");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        [HttpPut("ArchiveVacancy/{id}")]
        public ActionResult ArchiveVacancy(int id)
        {
            try
            {
            using (var context = new ProfnastavnikContext())
            {
                var vacancy = context.Vacancies.FirstOrDefault(x =>
                    x.IdVacancy == id);

                if (vacancy == null)
                {
                    return NotFound("вакансия не найдена");
                }


                vacancy.StatusVacancy = "Архив";
                
                context.SaveChanges();

                return Ok(vacancy);
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "VacancyController.ArchiveVacancy");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }



    }
}
