using Microsoft.AspNetCore.Mvc;
using ProfNastavnik.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ProfNastavnik.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        
        

        // PUT api/<ChatController>/5
        [HttpPut("MessageRead/{id}")]
        public ActionResult Put(int id)
        {
            using (var context = new ProfnastavnikContext())
            {
                Chat ch = context.Chats.FirstOrDefault(x => x.IdChat == id);
                if (ch == null)
                {
                    return NotFound("сообщение не найдено");
                }
                ch.StatusChat = "Прочитано";
                context.SaveChanges();
                return Ok(ch);
            }
        }

    }
}
