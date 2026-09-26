using Microsoft.AspNetCore.Mvc;
using ProfNastavnik.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ProfNastavnik.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserPricesController : ControllerBase
    {
        

        // GET api/<UserPricesController>/5
        [HttpGet("GetAllPrices")]
        public ActionResult Get()
        {
            using (var context = new ProfnastavnikContext())
            {
                List<PricesForUser> pr = context.PricesForUsers.ToList();
                return pr.Count() > 0 ? Ok(pr) : NotFound();
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


    }
}
