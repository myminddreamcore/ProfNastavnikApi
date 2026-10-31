using System;
using Microsoft.AspNetCore.Mvc;
using ProfNastavnik.Models;
using ProfNastavnik.Security;

namespace ProfNastavnik.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [TokenAuth]
    public class TariffController : ControllerBase
    {
        // GET api/Tariff/Me: действующий тариф, срок оплаты, лимиты и расход (по токену)
        [HttpGet("Me")]
        public ActionResult Me()
        {
            try
            {
                var auth = AuthContext.Get(HttpContext);
                if (auth == null) return StatusCode(401, "Требуется авторизация");
                if (auth.Role == "admin") return Ok(TariffService.Unlimited("admin"));
                using (var context = new ProfnastavnikContext())
                {
                    return Ok(TariffService.Compute(context, auth.Role, auth.UserId));
                }
            }
            catch (Exception ex)
            {
                ApiErrors.Log(ex, "TariffController.Me");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
    }
}
