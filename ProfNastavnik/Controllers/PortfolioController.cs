using System;
using ProfNastavnik.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ProfNastavnik.Models;

namespace ProfNastavnik.Controllers
{
  
    [Route("api/[controller]")]
    [Route("api/v1/[controller]")]
    [ApiController]
    [TokenAuth]
    public class PortfolioController : ControllerBase
    {
        private const string Sep = "||";
        private const int MaxItemsPerUser = 20;

        public class PortfolioItemDto
        {
            public int IdStudentFile { get; set; }
            public string Title { get; set; } = "";
            public string Url { get; set; } = "";
        }

        public class PortfolioAddDto
        {
            public string? Title { get; set; }
            public string? Url { get; set; }
        }

        private static PortfolioItemDto ToDto(StudentFile f)
        {
            string raw = f.FileUrl ?? "";
            int i = raw.IndexOf(Sep, StringComparison.Ordinal);
            if (i >= 0)
            {
                return new PortfolioItemDto
                {
                    IdStudentFile = f.IdStudentFile,
                    Title = raw.Substring(0, i),
                    Url = raw.Substring(i + Sep.Length)
                };
            }
            return new PortfolioItemDto { IdStudentFile = f.IdStudentFile, Title = raw, Url = raw };
        }

        // GET api/Portfolio/GetByUser/5
        [HttpGet("GetByUser/{userId}")]
        public ActionResult GetByUser(int userId)
        {
            try
            {
            using (var context = new ProfnastavnikContext())
            {
                var files = context.StudentFiles
                    .Where(x => x.IdUser == userId)
                    .OrderBy(x => x.IdStudentFile)
                    .ToList();
                return Ok(files.Select(ToDto).ToList());
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "PortfolioController.GetByUser");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }

        // POST api/Portfolio/Add/5   body: { "title": "...", "url": "https://..." }
        [HttpPost("Add/{userId}")]
        public ActionResult Add(int userId, [FromBody] PortfolioAddDto dto)
        {
            try
            {
            if (!AuthContext.IsSelfOrAdmin(HttpContext, "student", userId))
                return StatusCode(403, "Нет доступа");

            string title = (dto?.Title ?? "").Replace(Sep, " ").Trim();
            string url = (dto?.Url ?? "").Trim();

            if (title.Length == 0 || url.Length == 0)
                return BadRequest("Укажите название и ссылку");
            if (title.Length > 100)
                title = title.Substring(0, 100);
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                return BadRequest("Ссылка должна начинаться с http:// или https://");

            using (var context = new ProfnastavnikContext())
            {
                if (!context.Students.Any(x => x.IdStudent == userId))
                    return NotFound("Пользователь не найден");
                if (context.StudentFiles.Count(x => x.IdUser == userId) >= MaxItemsPerUser)
                    return BadRequest("Максимум " + MaxItemsPerUser + " работ в портфолио");

                var file = new StudentFile
                {
                    IdStudentFile = context.StudentFiles.Any() ? context.StudentFiles.Max(x => x.IdStudentFile) + 1 : 1,
                    IdUser = userId,
                    FileUrl = title + Sep + url
                };
                context.StudentFiles.Add(file);
                context.SaveChanges();
                return Ok(ToDto(file));
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "PortfolioController.Add");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }

        // DELETE api/Portfolio/Delete/12
        [HttpDelete("Delete/{fileId}")]
        public ActionResult Delete(int fileId)
        {
            try
            {
            using (var context = new ProfnastavnikContext())
            {
                var file = context.StudentFiles.FirstOrDefault(x => x.IdStudentFile == fileId);
                if (file == null) return NotFound("Работа не найдена");
                if (file.IdUser == null || !AuthContext.IsSelfOrAdmin(HttpContext, "student", file.IdUser.Value))
                    return StatusCode(403, "Нет доступа");
                context.StudentFiles.Remove(file);
                context.SaveChanges();
                return Ok(true);
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "PortfolioController.Delete");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
    }
}
