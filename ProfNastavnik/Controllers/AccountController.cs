using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfNastavnik.Models;
using ProfNastavnik.Security;

namespace ProfNastavnik.Controllers
{
    /// <summary>Удаление аккаунта и данных (требование Google Play и App Store; право на удаление по 152-ФЗ).</summary>
    [Route("api/[controller]")]
    [Route("api/v1/[controller]")]
    [ApiController]
    [TokenAuth]
    public class AccountController : ControllerBase
    {
        public class DeleteRequestDto
        {
            public string? Email { get; set; }
            public string? Password { get; set; }
        }

        // POST api/v1/Account/DeleteMe  body: { "password": "..." }; из приложения (пользователь авторизован токеном)
        [HttpPost("DeleteMe")]
        public ActionResult DeleteMe([FromBody] DeleteRequestDto dto)
        {
            try
            {
                var auth = AuthContext.Get(HttpContext);
                if (auth == null) return StatusCode(401, "Требуется авторизация");
                if (auth.Role == "admin") return BadRequest("Аккаунт администратора удалить нельзя");
                if (dto == null || string.IsNullOrEmpty(dto.Password)) return BadRequest("Введите пароль для подтверждения");
                if (LoginThrottle.IsLocked("del:" + auth.Email))
                    return StatusCode(429, "Слишком много попыток. Повторите позже");

                using (var context = new ProfnastavnikContext())
                {
                    var user = context.Users.FirstOrDefault(x => x.EmailUser == auth.Email);
                    if (user == null || !PasswordHasher.Verify(dto.Password, user.PasswordUser ?? ""))
                    {
                        LoginThrottle.Fail("del:" + auth.Email);
                        return StatusCode(403, "Неверный пароль");
                    }
                    LoginThrottle.Reset("del:" + auth.Email);
                    AccountService.HardDelete(context, auth.Role, auth.UserId, auth.Email);
                    return Ok(new { deleted = true });
                }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "AccountController.DeleteMe");
                return StatusCode(500, "Не удалось удалить аккаунт. Попробуйте позже или напишите в поддержку.");
            }
        }

        // POST api/v1/Account/DeleteByCredentials; для веб-страницы /legal/delete-account (удаление без установленного приложения)
        [PublicEndpoint]
        [SkipVersionCheck]
        [HttpPost("DeleteByCredentials")]
        public ActionResult DeleteByCredentials([FromBody] DeleteRequestDto dto)
        {
            try
            {
                if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrEmpty(dto.Password))
                    return BadRequest("Введите email и пароль");
                string key = "del:" + dto.Email.Trim();
                if (LoginThrottle.IsLocked(key)) return StatusCode(429, "Слишком много попыток. Повторите через 10 минут");

                using (var context = new ProfnastavnikContext())
                {
                    var user = context.Users.FirstOrDefault(x => x.EmailUser == dto.Email.Trim());
                    if (user == null || !PasswordHasher.Verify(dto.Password, user.PasswordUser ?? ""))
                    {
                        LoginThrottle.Fail(key);
                        return StatusCode(403, "Неверный email или пароль");
                    }
                    if (user.RoleUser == "Администратор") return BadRequest("Аккаунт администратора удалить нельзя");
                    LoginThrottle.Reset(key);

                    var student = context.Students.FirstOrDefault(x => x.EmailStudent == user.EmailUser);
                    var director = context.Directors.FirstOrDefault(x => x.EmailDirector == user.EmailUser);
                    if (student != null) AccountService.HardDelete(context, "student", student.IdStudent, user.EmailUser!);
                    else if (director != null) AccountService.HardDelete(context, "company", director.IdDirector, user.EmailUser!);
                    else AccountService.HardDelete(context, "none", 0, user.EmailUser!);  
                    return Ok(new { deleted = true });
                }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "AccountController.DeleteByCredentials");
                return StatusCode(500, "Не удалось удалить аккаунт. Попробуйте позже или напишите в поддержку.");
            }
        }
    }

    public static class AccountService
    {
        public static void HardDelete(ProfnastavnikContext db, string role, int id, string email)
        {
            using (var tx = db.Database.BeginTransaction())
            {
                if (role == "student") DeleteStudentData(db, id);
                else if (role == "company") DeleteCompanyData(db, id);

                db.SaveChanges();

                db.Deletings.RemoveRange(db.Deletings.Where(x => x.EmailUser == email));
                db.SaveChanges();

                if (role == "student")
                    db.Students.RemoveRange(db.Students.Where(x => x.EmailStudent == email));
                else if (role == "company")
                    db.Directors.RemoveRange(db.Directors.Where(x => x.EmailDirector == email));
                db.SaveChanges();

                var user = db.Users.FirstOrDefault(x => x.EmailUser == email);
                if (user != null && user.RoleUser != "Администратор")
                {
                    db.Users.Remove(user);
                    db.SaveChanges();
                }
                tx.Commit();
            }
        }

        private static void DeleteStudentData(ProfnastavnikContext db, int id)
        {
            var fbCompanyIds = db.FeedbacksCompanies.Where(x => x.IdUser == id).Select(x => x.IdFeedbackCompany).ToList();
            var fbUserIds = db.FeedbacksUsers.Where(x => x.IdUser == id).Select(x => x.IdFeedbackUser).ToList();
            db.RequestsUsers.RemoveRange(db.RequestsUsers.Where(x => x.IdStudent == id || (x.IdFeedback != null && fbCompanyIds.Contains(x.IdFeedback.Value))));
            db.RequestsCompanies.RemoveRange(db.RequestsCompanies.Where(x => x.IdFeedback != null && fbUserIds.Contains(x.IdFeedback.Value)));
            db.SaveChanges();

            db.FeedbacksCompanies.RemoveRange(db.FeedbacksCompanies.Where(x => x.IdUser == id));
            db.FeedbacksUsers.RemoveRange(db.FeedbacksUsers.Where(x => x.IdUser == id));
            db.Interships.RemoveRange(db.Interships.Where(x => x.IdUser == id));
            db.SaveChanges();

            db.Responses.RemoveRange(db.Responses.Where(x => x.IdUser == id));
            db.Favourities.RemoveRange(db.Favourities.Where(x => x.IdUser == id));
            db.Chats.RemoveRange(db.Chats.Where(x => x.IdUser == id));
            db.StudentFiles.RemoveRange(db.StudentFiles.Where(x => x.IdUser == id));
            db.UserSkills.RemoveRange(db.UserSkills.Where(x => x.IdUser == id));
            db.UserProfessions.RemoveRange(db.UserProfessions.Where(x => x.IdUser == id));
            db.UserPrices.RemoveRange(db.UserPrices.Where(x => x.IdUser == id));
            foreach (var pay in db.UserPays.Where(x => x.IdUser == id).ToList()) pay.IdUser = null;
        }

        private static void DeleteCompanyData(ProfnastavnikContext db, int id)
        {
            var vacancyIds = db.Vacancies.Where(x => x.IdCompany == id).Select(x => x.IdVacancy).ToList();

            var fbUserIds = db.FeedbacksUsers.Where(x => x.IdCompany == id || (x.IdVacancy != null && vacancyIds.Contains(x.IdVacancy.Value)))
                .Select(x => x.IdFeedbackUser).ToList();
            var fbCompanyIds = db.FeedbacksCompanies.Where(x => x.IdCompany == id || (x.IdVacancy != null && vacancyIds.Contains(x.IdVacancy.Value)))
                .Select(x => x.IdFeedbackCompany).ToList();
            db.RequestsCompanies.RemoveRange(db.RequestsCompanies.Where(x => x.IdCompany == id || (x.IdFeedback != null && fbUserIds.Contains(x.IdFeedback.Value))));
            db.RequestsUsers.RemoveRange(db.RequestsUsers.Where(x => x.IdFeedback != null && fbCompanyIds.Contains(x.IdFeedback.Value)));
            db.SaveChanges();

            db.FeedbacksUsers.RemoveRange(db.FeedbacksUsers.Where(x => fbUserIds.Contains(x.IdFeedbackUser)));
            db.FeedbacksCompanies.RemoveRange(db.FeedbacksCompanies.Where(x => fbCompanyIds.Contains(x.IdFeedbackCompany)));
            db.Interships.RemoveRange(db.Interships.Where(x => x.IdCompany == id || (x.IdVacancy != null && vacancyIds.Contains(x.IdVacancy.Value))));
            db.SaveChanges();

            db.Responses.RemoveRange(db.Responses.Where(x => x.IdCompany == id || (x.IdVacancy != null && vacancyIds.Contains(x.IdVacancy.Value))));
            db.Chats.RemoveRange(db.Chats.Where(x => x.IdDirector == id || (x.IdVacancy != null && vacancyIds.Contains(x.IdVacancy.Value))));
            db.Favourities.RemoveRange(db.Favourities.Where(x => x.IdVacancy != null && vacancyIds.Contains(x.IdVacancy.Value)));
            db.VacancySkills.RemoveRange(db.VacancySkills.Where(x => x.IdVacancy != null && vacancyIds.Contains(x.IdVacancy.Value)));
            db.VacancySferes.RemoveRange(db.VacancySferes.Where(x => x.IdVacancy != null && vacancyIds.Contains(x.IdVacancy.Value)));
            db.VacanciesFormats.RemoveRange(db.VacanciesFormats.Where(x => x.IdVacancy != null && vacancyIds.Contains(x.IdVacancy.Value)));
            db.Adresses.RemoveRange(db.Adresses.Where(x => x.IdVacancy != null && vacancyIds.Contains(x.IdVacancy.Value)));
            db.SaveChanges();

            db.Vacancies.RemoveRange(db.Vacancies.Where(x => x.IdCompany == id));
            db.CompanySferes.RemoveRange(db.CompanySferes.Where(x => x.IdCompany == id));
            db.CompanyPrices.RemoveRange(db.CompanyPrices.Where(x => x.IdCompany == id));
            foreach (var pay in db.CompanyPays.Where(x => x.IdCompany == id).ToList()) pay.IdCompany = null;
        }
    }
}
