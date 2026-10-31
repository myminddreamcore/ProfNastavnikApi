using System;
using ProfNastavnik.Security;
using Microsoft.AspNetCore.Mvc;
using ProfNastavnik.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ProfNastavnik.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [TokenAuth]
    public class UserPricesController : ControllerBase
    {
        

        // GET api/<UserPricesController>/5
        [HttpGet("GetAllPrices")]
        public ActionResult Get()
        {
            try
            {
            using (var context = new ProfnastavnikContext())
            {
                List<PricesForUser> pr = context.PricesForUsers.ToList();
                return pr.Count() > 0 ? Ok(pr) : NotFound();
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "UserPricesController.Get");
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
                ApiErrors.Log(__ex, "UserPricesController.AddNewUserPrice");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }


    }
}
