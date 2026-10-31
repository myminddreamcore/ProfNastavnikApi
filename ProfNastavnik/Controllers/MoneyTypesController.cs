using System;
using ProfNastavnik.Security;
using Microsoft.AspNetCore.Mvc;
using ProfNastavnik.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ProfNastavnik.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoneyTypesController : ControllerBase
    {
      

        // GET api/<MoneyTypesController>/5
        [HttpGet("GetMoneyTypes")]
        public ActionResult Get()
        {
            try
            {
            using (var context = new ProfnastavnikContext())
            {
                List<MoneyType> m = context.MoneyTypes.ToList();
                return m.Count > 0 ? Ok(m) : NotFound();
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "MoneyTypesController.Get");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }

    }
}
