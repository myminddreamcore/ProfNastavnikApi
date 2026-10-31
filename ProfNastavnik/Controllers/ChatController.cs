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
    public class ChatController : ControllerBase
    {
        
        

        // PUT api/<ChatController>/5
        [HttpPut("MessageRead/{id}")]
        public ActionResult Put(int id)
        {
            try
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
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "ChatController.Put");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }

    }
}
