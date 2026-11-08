using System;
using ProfNastavnik.Security;
using Microsoft.AspNetCore.Mvc;
using ProfNastavnik.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ProfNastavnik.Controllers
{
    [Route("api/[controller]")]
    [Route("api/v1/[controller]")]
    [ApiController]
    [TokenAuth]
    public class InterShipController : ControllerBase
    {
        

        // GET api/<InterShipController>/5
        [HttpGet("UserIntership/{id_user}/{id_company}/{id_vacancy}")]
        public ActionResult Get(int id_user, int id_company, int id_vacancy)
        {
            try
            {
            using (var context = new ProfnastavnikContext())
            {
                Intership i = context.Interships.FirstOrDefault(x => x.IdVacancy == id_vacancy && x.IdCompany == id_company && x.IdUser == id_user);
                if (i == null) 
                {
                    return NotFound("стажировка не найдена");
                }
                return Ok(i);


            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "InterShipController.Get");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        [HttpGet("GetInternshipDays/{id}")]
        public ActionResult GetInternshipDays(int id)
        {
            try
            {
            using (var context = new ProfnastavnikContext())
            {
                var internship = context.Interships.FirstOrDefault(x => x.IdIntership == id);
                if (internship == null)
                {
                    return NotFound("Стажировка не найдена");
                }

                if (internship.StatusIntership != "Идет")
                {
                    return Ok(0);
                }

                var days = (DateTime.Now.Date - internship.DateStartIntership?.ToDateTime(TimeOnly.MinValue)).Value.Days;
                return Ok(days);
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "InterShipController.GetInternshipDays");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }

        // POST api/<InterShipController>
        [HttpPost("NewInterShip")]
        public ActionResult Post(Intership i)
        {
            try
            {
            using (var context = new ProfnastavnikContext())
            {
                Intership old = context.Interships.FirstOrDefault(x => x.IdVacancy == i.IdVacancy && x.IdCompany == i.IdCompany && x.IdUser == i.IdUser);
                if (old != null)
                {
                    return NotFound("стажировка уже есть найдена");
                }
                Response r = context.Responses.FirstOrDefault(x => x.IdVacancy == i.IdVacancy && x.IdCompany == i.IdCompany && x.IdUser == i.IdUser);
                r.StatusResponse = "Приглашен";
                i.IdIntership = context.Interships.Any() ? context.Interships.Max(x => x.IdIntership) + 1 : 1;
                i.DateStartIntership = DateOnly.FromDateTime(DateTime.Now);
                i.StatusIntership = "Идет";
                i.Response = r;
                context.Interships.Add(i);
                context.SaveChanges();
                return Ok(i);




            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "InterShipController.Post");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }

        // PUT api/<InterShipController>/5
        [HttpPut("EndInterShip/{id}")]
        public ActionResult Put(int id)
        {
            try
            {
            using (var context = new ProfnastavnikContext())
            {
                Intership i = context.Interships.FirstOrDefault(x => x.IdIntership==id);
                if (i == null)
                {
                    return NotFound("стажировка не найдена");
                }
                i.StatusIntership = "Закончена";
                i.DateEndIntership= DateOnly.FromDateTime(DateTime.Now);
                context.SaveChanges();
                return Ok(i);


            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "InterShipController.Put");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }

        
    }
}
