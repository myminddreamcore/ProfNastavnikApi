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
    public class UniversityController : ControllerBase
    {
       

        // GET api/<UniversityController>/5
        [HttpGet("GetAllUniversity")]
        public ActionResult Get()
        {
            try
            {
            using (var context = new ProfnastavnikContext())
            {
                return Ok(context.Universities.ToList());
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "UniversityController.Get");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }

    }
}
