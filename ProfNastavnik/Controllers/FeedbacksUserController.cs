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
    public class FeedbacksUserController : ControllerBase
    {


        [HttpGet("GetUsersFeedbacks/{id}")]
        public ActionResult GetUsersFeedbacks(int id)
        {
            try
            {
            using (var context = new ProfnastavnikContext())
            {
                List<FeedbacksUserDTO> feaads = context.FeedbacksUsers.Include(x => x.IdVacancyNavigation).Include(x => x.IdCompanyNavigation).Where(x => x.IdUser == id && x.StatusFeedbackUser== "Одобрен").Select(x => new FeedbacksUserDTO
                {
                    f = x,
                    NameCompany = x.IdCompanyNavigation.NameCompanyDirector,
                    NameVacancy = x.IdVacancyNavigation.NameVacancy,
                }).ToList();
                return feaads.Count() > 0 ? Ok(feaads) : NotFound();
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "FeedbacksUserController.GetUsersFeedbacks");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        // GET api/<FeedbacksUserController>/5
        [HttpGet("GetFeedback/{id}")]
        public ActionResult GetFeedbackGetUsersFeedbacks(int id)
        {
            try
            {
            using (var context = new ProfnastavnikContext())
            {
                var feedback = context.FeedbacksUsers
                    .Include(x => x.IdVacancyNavigation)
                    .Include(x => x.IdCompanyNavigation)
                    .FirstOrDefault(x => x.IdFeedbackUser == id);

                if (feedback != null)
                {
                    var dto = new FeedbacksUserDTO
                    {
                        f = feedback,
                        NameCompany = feedback.IdCompanyNavigation?.NameCompanyDirector,
                        NameVacancy = feedback.IdVacancyNavigation?.NameVacancy
                    };
                    return Ok(dto);
                }
                return NotFound();
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "FeedbacksUserController.GetFeedbackGetUsersFeedbacks");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        [HttpPost("AddFeedbackUser")]
        public ActionResult AddComplaint(FeedbacksUser c)
        {
            try
            {
            using (var context = new ProfnastavnikContext())
            {
                FeedbacksUser current = context.FeedbacksUsers.FirstOrDefault(x => x.IdCompany == c.IdCompany && x.IdUser == c.IdUser && x.IdVacancy == c.IdVacancy);
                if (current != null)
                {
                    return BadRequest("вы уже оставляли отзыв на эту вакансию");
                }
                c.StatusFeedbackUser = "На модерации";
                c.DateFeedback = DateOnly.FromDateTime(DateTime.UtcNow);
                c.IdFeedbackUser = context.FeedbacksUsers.Any() ? context.FeedbacksUsers.Max(x => x.IdFeedbackUser) + 1 : 1;
                context.FeedbacksUsers.Add(c);
                context.SaveChanges();
                return Ok(c);
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "FeedbacksUserController.AddComplaint");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        [HttpPut("DeleteFeedbackUser/{id}")]
        public ActionResult DeleteFeedbackUser(int id)
        {
            try
            {
            using (var context = new ProfnastavnikContext())
            {
                FeedbacksUser current = context.FeedbacksUsers.FirstOrDefault(x => x.IdFeedbackUser==id);
                if (current == null)
                {
                    return NotFound("жалоба не найдена");
                }
                current.StatusFeedbackUser = "Удален";
                context.SaveChanges();
                return Ok(current);
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "FeedbacksUserController.DeleteFeedbackUser");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }


    }
}
