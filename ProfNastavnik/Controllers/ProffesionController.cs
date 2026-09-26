using Microsoft.AspNetCore.Mvc;
using ProfNastavnik.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ProfNastavnik.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProffesionController : ControllerBase
    {
       

        // GET api/<ProffesionController>/5
        [HttpGet("GetAllProffesions")]
        public ActionResult Get()
        {
            using (var context = new ProfnastavnikContext())
            {
                return Ok(context.Professions.ToList());
            }
        }

       
    }
}
