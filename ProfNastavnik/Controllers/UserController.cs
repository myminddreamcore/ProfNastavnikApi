using System;
using ProfNastavnik.Security;

using MailKit.Security;
using MimeKit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfNastavnik.DTO;
using ProfNastavnik.Models;
using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Xml.Linq;
using System.Net;
using System.Net.Mail;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ProfNastavnik.Controllers
{
    [Route("api/[controller]")]
    [Route("api/v1/[controller]")]
    [ApiController]
    [TokenAuth]
    public class UserController : ControllerBase
    {

       

        [PublicEndpoint]
        [HttpPost("ResetPassword")]
        public ActionResult ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
            if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.NewPassword))
                return BadRequest("Некорректный запрос");
            if (!EmailVerifiedStore.Consume(request.Email))
                return StatusCode(403, "Сначала подтвердите email кодом из письма");
            string hashedPassword = PasswordHasher.Hash(request.NewPassword);

            using (var context = new ProfnastavnikContext())
            {
                var user = context.Users.FirstOrDefault(x => x.EmailUser == request.Email);
                if (user == null)
                    return NotFound("Пользователь не найден");

                user.PasswordUser = hashedPassword;
                context.SaveChanges();
                return Ok();
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "UserController.ResetPassword");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }

        public class ResetPasswordRequest
        {
            public string Email { get; set; }
            public string NewPassword { get; set; }
        }
        [PublicEndpoint]
        [HttpPost("Authorization")]
        public ActionResult Post(User s)
        {
            try
            {
            if (s == null || string.IsNullOrWhiteSpace(s.EmailUser) || string.IsNullOrEmpty(s.PasswordUser))
                return BadRequest("Введите email и пароль");
            if (LoginThrottle.IsLocked(s.EmailUser))
                return StatusCode(429, "Слишком много неудачных попыток входа. Повторите через 10 минут");

            using (var context = new ProfnastavnikContext())
            {
                User user = context.Users.FirstOrDefault(x => x.EmailUser == s.EmailUser);
                if (user == null || !PasswordHasher.Verify(s.PasswordUser, user.PasswordUser ?? ""))
                {
                    LoginThrottle.Fail(s.EmailUser);
                    return NotFound("пользователь не найден");
                }
                LoginThrottle.Reset(s.EmailUser);
                if (PasswordHasher.NeedsRehash(user.PasswordUser ?? ""))
                {
                    user.PasswordUser = PasswordHasher.Hash(s.PasswordUser);
                    context.SaveChanges();
                }
                if (user.RoleUser == "Администратор")
                {
                    return IssueToken(user, "admin", 1);
                }
                Student st = context.Students.FirstOrDefault(x => x.EmailStudent == s.EmailUser);
                Director d = context.Directors.FirstOrDefault(x => x.EmailDirector == s.EmailUser);
                if (st != null)
                {
                    if (st.StatusStudent == "Активен" || st.StatusStudent == "На модерации")
                    {
                        return IssueToken(user, "student", st.IdStudent);
                    }
                    return BadRequest("пользователь недействителен");
                }
                else if (d != null)
                {
                    if (d.StatusDirector == "Активен" || d.StatusDirector == "На модерации")
                    {
                        return IssueToken(user, "company", d.IdDirector);
                    }
                    return BadRequest("пользователь недействителен");

                }

                else
                {
                    return BadRequest("пользователь недействителен");
                }
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "UserController.Post");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }

        private ActionResult IssueToken(User user, string role, int id)
        {
            string token = TokenService.Issue(id, user.EmailUser ?? "", role);
            HttpContext.Response.Headers["X-Auth-Token"] = token;
            user.PasswordUser = "";   
            return Ok(user);
        }

        private string Sha256Hash(string input)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(input);
                var hash = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
      
        [HttpGet("GetUser/{email}")]
        public ActionResult GetUser(string email)
        {
            try
            {
            using (var context = new ProfnastavnikContext())
            {
                Student user = context.Students.FirstOrDefault(x => x.EmailStudent == email);
                return user != null ? Ok(user) : NotFound("пользователь не найден");
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "UserController.GetUser");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        [HttpGet("GetUserById/{id}")]
        public ActionResult GetUser(int id)
        {
            try
            {
            using (var context = new ProfnastavnikContext())
            {
                Student user = context.Students.FirstOrDefault(x => x.IdStudent == id);
                return user != null ? Ok(user) : NotFound("пользователь не найден");
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "UserController.GetUser");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        [HttpGet("GetUserPrice/{id}")]
        public ActionResult GetUserPrice(int id)
        {
            try
            {
            using (var context = new ProfnastavnikContext())
            {
                UserPrice user = context.UserPrices.FirstOrDefault(x => x.IdUser == id);
                if (user == null) return NotFound("не найден прайс");
                var tariff = TariffService.Compute(context, "student", id);
                return Ok(new UserPrice { Id = user.Id, IdUser = user.IdUser, IdPrices = tariff.TariffId });
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "UserController.GetUserPrice");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        [HttpGet("GetUserCountResponce/{id}")]
        public ActionResult GetUserCountResponce(int id)
        {
            try
            {
                if (!AuthContext.IsSelfOrAdmin(HttpContext, "student", id))
                    return StatusCode(403, "Нет доступа");
            using (var context = new ProfnastavnikContext())
            {
                DateOnly date = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);
                List<Response> user = context.Responses.Where(x => x.IdUser == id && x.DateResponse>=date).ToList();
                return user.Count()>0 ? Ok(user.Count) : NotFound("не найдены");
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "UserController.GetUserCountResponce");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        [HttpGet("GetUserChats/{id}")]
        public ActionResult GetUserChats(int id)
        {
            try
            {
                if (!AuthContext.IsSelfOrAdmin(HttpContext, "student", id))
                    return StatusCode(403, "Нет доступа");
            using (var context = new ProfnastavnikContext())
            {
                var chats = context.Chats.Include(x=>x.IdVacancyNavigation).Include(x=>x.IdDirectorNavigation).Include(x=>x.IdUserNavigation)
                    .Where(x => x.IdUser == id)
                    .OrderByDescending(x => x.SendAtChat)
                    .ToList();

                var itog = chats
                    .GroupBy(chat => chat.IdDirector ?? (object)chat.EmailAdmin) 
                    .Select(g => g.First()).Select(x=> new ChatDTO
                    {
                        Chat = x,
                        NameCompany = x.IdDirectorNavigation?.NameCompanyDirector,
                        NameVacancy = x.IdVacancyNavigation?.NameVacancy,
                        FIOUser = x.IdUserNavigation.SurnameStudent + " "+ x.IdUserNavigation.NameStudent+" "+ x.IdUserNavigation.PatronymicStudent
                    })
                    .ToList();

                return itog.Count > 0 ? Ok(itog) : NotFound("чаты не найдены");
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "UserController.GetUserChats");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }

        [HttpGet("GetChatMessagesCompany/{id_user}/{id_company}/{id_vacancy}")]
        public ActionResult GetChatMessages(int id_user, int id_company, int id_vacancy)
        {
            try
            {
                if (!AuthContext.IsParticipant(HttpContext, id_user, id_company))
                    return StatusCode(403, "Нет доступа");
            using (var context = new ProfnastavnikContext())
            {
                if (id_user != 0 && id_vacancy != 0)
                {
                    List<Chat> chats = context.Chats.Include(x => x.IdUserNavigation).Include(x => x.IdDirectorNavigation).Include(x => x.IdVacancyNavigation).Where(x => x.IdUser == id_user && x.IdVacancy == id_vacancy && x.IdDirector == id_company).OrderBy(x => x.SendAtChat).ToList();
                    var itog = chats.Select(x => new ChatDTO
                    {
                        Chat = x,
                        NameCompany = x.IdDirectorNavigation?.NameCompanyDirector,
                        NameVacancy = x.IdVacancyNavigation?.NameVacancy,
                        FIOUser = x.IdUserNavigation.SurnameStudent + " " + x.IdUserNavigation.NameStudent + " " + x.IdUserNavigation.PatronymicStudent
                    });
                    return itog.Count() > 0 ? Ok(itog) : NotFound("чаты не найдены");
                }
                else
                {
                    List<Chat> chats = context.Chats.Include(x => x.IdUserNavigation).Include(x => x.IdDirectorNavigation).Include(x => x.IdVacancyNavigation).Where(x =>  x.IdDirector == id_company).OrderBy(x => x.SendAtChat).ToList();
                    var itog = chats.Select(x => new ChatDTO
                    {
                        Chat = x,
                        NameCompany = null,
                        NameVacancy = null,
                        FIOUser = null
                    });
                    return itog.Count() > 0 ? Ok(itog) : NotFound("чаты не найдены");
                }
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "UserController.GetChatMessages");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        [HttpGet("GetChatMessagesCompanyAdmin/{id_company}/{email_admin}")]
        public ActionResult GetChatMessagesCompanyAdmin(int id_company, string email_admin)
        {
            try
            {
                if (!AuthContext.IsSelfOrAdmin(HttpContext, "company", id_company))
                    return StatusCode(403, "Нет доступа");
            using (var context = new ProfnastavnikContext())
            {
                
                    List<Chat> chats = context.Chats.Include(x => x.IdUserNavigation).Include(x => x.IdDirectorNavigation).Include(x => x.IdVacancyNavigation).Where(x => x.IdDirector == id_company && x.EmailAdmin==email_admin).OrderBy(x => x.SendAtChat).ToList();
                    var itog = chats.Select(x => new ChatDTO
                    {
                        Chat = x,
                        NameCompany = null,
                        NameVacancy = null,
                        FIOUser = null
                    });
                    return itog.Count() > 0 ? Ok(itog) : NotFound("чаты не найдены");
                
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "UserController.GetChatMessagesCompanyAdmin");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        [HttpGet("GetChatMessagesAdmin/{id_user}/{email_admin}")]
        public ActionResult GetChatMessages(int id_user, string email_admin)
        {
            try
            {
                if (!AuthContext.IsSelfOrAdmin(HttpContext, "student", id_user))
                    return StatusCode(403, "Нет доступа");
            using (var context = new ProfnastavnikContext())
            {
               
                    List<Chat> chats = context.Chats.Include(x => x.IdUserNavigation).Include(x => x.IdDirectorNavigation).Include(x => x.IdVacancyNavigation).Where(x => x.IdUser == id_user && x.EmailAdmin == email_admin).OrderBy(x => x.SendAtChat).ToList();
                    var itog = chats.Select(x => new ChatDTO
                    {
                        Chat = x,
                        NameCompany = x.IdDirectorNavigation?.NameCompanyDirector,
                        NameVacancy = x.IdVacancyNavigation?.NameVacancy,
                        FIOUser = x.IdUserNavigation.SurnameStudent + " " + x.IdUserNavigation.NameStudent + " " + x.IdUserNavigation.PatronymicStudent
                    });
                    return itog.Count() > 0 ? Ok(itog) : NotFound("чаты не найдены");
                
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "UserController.GetChatMessages");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        [HttpPost("SendMessage")]
        public ActionResult SendMessage(Chat t)
        {
            try
            {
            var auth = AuthContext.Get(HttpContext);
            if (auth == null) return StatusCode(401, "Требуется авторизация");
            if (t == null || string.IsNullOrWhiteSpace(t.TextChat)) return BadRequest("Пустое сообщение");
            if (t.TextChat.Length > 2000) return BadRequest("Сообщение слишком длинное (максимум 2000 символов)");

            using (var context = new ProfnastavnikContext())
            {
                switch (t.SenderChat)
                {
                    case "Админ":
                        if (auth.Role != "admin") return StatusCode(403, "Нет доступа");
                        break;
                    case "Стажер":
                        if (auth.Role != "admin" && (auth.Role != "student" || t.IdUser != auth.UserId))
                            return StatusCode(403, "Нет доступа");
                        break;
                    case "Работодатель":
                        if (auth.Role != "admin")
                        {
                            if (auth.Role != "company" || t.IdDirector != auth.UserId)
                                return StatusCode(403, "Нет доступа");

                            bool toAdmin = !string.IsNullOrWhiteSpace(t.EmailAdmin);
                            if (!toAdmin)
                            {
                                if (t.IdUser == null) return BadRequest("Не указан получатель");
                                bool dialogExists = context.Chats.Any(x => x.IdDirector == auth.UserId && x.IdUser == t.IdUser
                                                                           && (x.EmailAdmin == null || x.EmailAdmin == ""));
                                if (!dialogExists)
                                {
                                    var tariff = TariffService.Compute(context, "company", auth.UserId);
                                    if (!tariff.InviteStudents)
                                        return StatusCode(403, tariff.Locked("Приглашение стажёров (писать первыми)"));
                                }
                            }
                        }
                        break;
                    default:
                        return BadRequest("Неизвестный отправитель");
                }

                t.SendAtChat = DateTime.Now;
                t.IdChat = context.Chats.Any() ? context.Chats.Max(x => x.IdChat) + 1 : 1;
                context.Chats.Add(t);
                context.SaveChanges();
                return Ok(t);
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "UserController.SendMessage");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        [HttpGet("GetUserArchiveVacancies/{id}")]
        public ActionResult GetUserArchiveVacancies(int id)
        {
            try
            {
                if (!AuthContext.IsSelfOrAdmin(HttpContext, "student", id))
                    return StatusCode(403, "Нет доступа");
            using (var context = new ProfnastavnikContext())
            {
                List<Intership> list = context.Interships.Where(x => x.IdUser == id && x.StatusIntership == "Закончена").Include(x => x.IdVacancyNavigation).Include(x => x.IdVacancyNavigation.CurrencyVacancyNavigation).ToList();
                var list2 = list.Select(x => new ListIntership
                {
                    idVacancy = x.IdVacancy,
                    NameVacancy = x.IdVacancyNavigation?.NameVacancy,
                    idCompany = x.IdCompany,
                    ZenStart = x.IdVacancyNavigation?.ZenStartVacancy,
                    Currency = x.IdVacancyNavigation?.CurrencyVacancyNavigation?.NameCurrency,
                    ZenEnd = x.IdVacancyNavigation?.ZenEndVacancy,
                    Time = x.IdVacancyNavigation?.TimeVacancy

                }).ToList();
                return list2.Count() > 0 ? Ok(list2) : NotFound("архивные вакансии не найдены");
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "UserController.GetUserArchiveVacancies");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        [HttpGet("GetUserFaivorities/{id}")]
        public ActionResult GetUserFaivorities(int id)
        {
            try
            {
                if (!AuthContext.IsSelfOrAdmin(HttpContext, "student", id))
                    return StatusCode(403, "Нет доступа");
            using (var context = new ProfnastavnikContext())
            {
                List<Favourity> list = context.Favourities.Where(x => x.IdUser == id).Include(x => x.IdVacancyNavigation).Include(x => x.IdVacancyNavigation.CurrencyVacancyNavigation).ToList();
                var list2 = list.Select(x => new ListIntership
                {
                    idVacancy = x.IdVacancy,
                    NameVacancy = x.IdVacancyNavigation?.NameVacancy,
                    ZenStart = x.IdVacancyNavigation?.ZenStartVacancy,
                    Currency = x.IdVacancyNavigation?.CurrencyVacancyNavigation?.NameCurrency,
                    ZenEnd = x.IdVacancyNavigation?.ZenEndVacancy,
                    Time = x.IdVacancyNavigation?.TimeVacancy

                }).ToList();
                return list2.Count() > 0 ? Ok(list2) : NotFound("вакансии не найдены");

            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "UserController.GetUserFaivorities");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        [HttpPut("DeleteUser/{id}")]
        public ActionResult DeleteUser(int id)
        {
            try
            {
            if (!AuthContext.IsSelfOrAdmin(HttpContext, "student", id))
                return StatusCode(403, "Нет доступа");
            using (var context = new ProfnastavnikContext())
            {
                Student user = context.Students.FirstOrDefault(x => x.IdStudent == id);
                if (user == null)
                {
                    return NotFound("пользователь не найден");
                }
                user.StatusStudent = "Удален";
                List<Intership> inter = context.Interships.Where(x => x.IdUser == user.IdStudent).ToList();
                foreach (var intership in inter)
                {
                    intership.StatusIntership = "Закончена";
                    intership.DateEndIntership = DateOnly.FromDateTime(DateTime.Now);

                }
                List<Response> resp = context.Responses.Where(x => x.IdUser == user.IdStudent).ToList();
                foreach (var response in resp)
                {
                    response.StatusResponse = "Отклонено";
                }
               
                context.SaveChanges();
                return Ok(user) ;

            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "UserController.DeleteUser");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        [HttpGet("GetAllUserProfile/{id}")]
        public ActionResult GetAllUserProfile(int id)
        {
            try
            {
            using (var context = new ProfnastavnikContext())
            {
                Student list = context.Students.Include(x=>x.UniversityStudentNavigation).FirstOrDefault(x => x.IdStudent == id);


                List<string> skills = new List<string>();
                List<UserSkill> allskills = context.UserSkills.Include(x=>x.IdSkillNavigation).Where(x=>x.IdUser == id).ToList();
                foreach (var skill in allskills)
                {
                    skills.Add(skill.IdSkillNavigation.NameSkill);
                }

                List<string> proffesions = new List<string>();
                List<UserProfession> prof = context.UserProfessions.Include(x => x.IdProfessionNavigation).Where(x => x.IdUser == id).ToList();
                foreach (var pro in prof)
                {
                    proffesions.Add(pro.IdProfessionNavigation.NameProfession);
                }


                List<string> studfiles = new List<string>();
                List<StudentFile> studentfiles = context.StudentFiles.Where(x => x.IdUser == id).ToList();
                foreach (var pro in studentfiles)
                {
                    studfiles.Add(pro.FileUrl);
                }
                AllUserprofile all = new AllUserprofile()
                {
                    student = list,
                    University = list.UniversityStudentNavigation?.NameUniversity,
                    Skills = skills,
                    Proffesions = proffesions,
                    StudentFiles = studfiles

                };
                return all!=null ? Ok(all) : NotFound("пользователь не найдены");

            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "UserController.GetAllUserProfile");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }



        [PublicEndpoint]
        [HttpPost("SendVerificationCode")]
        public ActionResult SendVerificationCode([FromBody] EmailRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Email))
                    return BadRequest(new { message = "Не указан email" });
                if (!SendThrottle.TryAcquire(request.Email))
                    return StatusCode(429, new { message = "Код уже отправлен. Повторите через минуту" });
                var code = System.Security.Cryptography.RandomNumberGenerator.GetInt32(1000, 10000).ToString();
                VerificationCodeStorage.SaveCode(request.Email, code);

                string fromEmail = "official.workship@mail.ru";
                string password = Environment.GetEnvironmentVariable("SMTP_PASSWORD") ?? "";
                string smtpServer = "smtp.mail.ru";
                int port = 587;

                using (var client = new SmtpClient(smtpServer, port))
                {
                    client.EnableSsl = true;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(fromEmail, password);
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.Timeout = 30000;

                    using (var mail = new MailMessage())
                    {
                        mail.From = new MailAddress(fromEmail, "Воркшип");
                        mail.To.Add(request.Email);
                        mail.Subject = "Код подтверждения Воркшип";
                        mail.Body = $@"
                    <html>
                        <body>
                            <h2>Код подтверждения</h2>
                            <p>Ваш код: <strong>{code}</strong></p>
                            <p>Код действителен 10 минут.</p>
                        </body>
                    </html>";
                        mail.IsBodyHtml = true;

                        client.Send(mail);
                    }
                }

                return Ok(new { message = "Код отправлен на email" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка: {ex.Message}");
                if (ex.InnerException != null)
                    System.Diagnostics.Debug.WriteLine($"Внутренняя ошибка: {ex.InnerException.Message}");

                return StatusCode(500, $"Ошибка отправки: {ex.Message}");
            }
        }

        [PublicEndpoint]
        [HttpPost("VerifyCode")]
        public ActionResult VerifyCode([FromBody] VerifyCodeRequest request)
        {
            try
            {
            if (request == null || string.IsNullOrWhiteSpace(request.Email))
                return BadRequest(new { message = "Не указан email" });
            string throttleKey = "code:" + request.Email;
            if (LoginThrottle.IsLocked(throttleKey))
                return StatusCode(429, new { message = "Слишком много неверных попыток. Запросите новый код через 10 минут" });

            bool isValid = VerificationCodeStorage.VerifyCode(request.Email, request.Code);

            if (isValid)
            {
                LoginThrottle.Reset(throttleKey);
                VerificationCodeStorage.RemoveCode(request.Email);
                EmailVerifiedStore.Mark(request.Email);
                return Ok(new { message = "Код успешно подтвержден" });
            }

            LoginThrottle.Fail(throttleKey);
            return BadRequest(new { message = "Неверный или истекший код" });
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "UserController.VerifyCode");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        [PublicEndpoint]
        [HttpPost("Register")]
        public ActionResult Register([FromBody] User user)
        {
            try
            {
            if (user == null || string.IsNullOrWhiteSpace(user.EmailUser) || !user.EmailUser.Contains('@'))
                return BadRequest("Некорректный email");
            if (string.IsNullOrEmpty(user.PasswordUser) || user.PasswordUser.Length < 6)
                return BadRequest("Пароль должен быть не короче 6 символов");
            // Согласие на обработку ПД и принятие документов: отдельные отметки в приложении (152-ФЗ, ст. 9)
            string consentVersion = Request.Headers["X-Consent-Version"].ToString();
            bool adultConfirmed = Request.Headers["X-Adult-Confirmed"].ToString() == "true";
            if (string.IsNullOrWhiteSpace(consentVersion) || !adultConfirmed)
                return BadRequest("Необходимо принять условия и дать согласие на обработку персональных данных");
            // Публично можно зарегистрироваться только как стажёр или работодатель.
            // Раньше роль бралась из запроса, и любой мог создать аккаунт «Администратор».
            if (user.RoleUser != "Стажер" && user.RoleUser != "Работодатель")
                return BadRequest("Недопустимая роль");

            using (var context = new ProfnastavnikContext())
            {
                var existingUser = context.Users.FirstOrDefault(x => x.EmailUser == user.EmailUser);
                if (existingUser != null)
                {
                    return BadRequest("Пользователь с таким email уже существует");
                }

                user.PasswordUser = PasswordHasher.Hash(user.PasswordUser);

                context.Users.Add(user);
                
                
                context.SaveChanges();

                ConsentStore.Record(context, user.EmailUser, consentVersion, HttpContext.Connection.RemoteIpAddress?.ToString(), adultConfirmed);
                user.PasswordUser = "";
                return Ok(user);
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "UserController.Register");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        public class EmailRequest
        {
            public string Email { get; set; }
        }

        public class VerifyCodeRequest
        {
            public string Email { get; set; }
            public string Code { get; set; }
        }

        public class EmailVerification
        {
            public int Id { get; set; }
            public string Email { get; set; }
            public string Code { get; set; }
            public DateTime CreatedAt { get; set; }
            public bool IsUsed { get; set; }
        }

        [PublicEndpoint]
        [HttpPost("CreateStudent")]
        public ActionResult CreateStudent([FromBody] Student student)
        {
            try
            {
            if (student == null || string.IsNullOrWhiteSpace(student.EmailStudent))
                return BadRequest("Не указан email");
            using (var context = new ProfnastavnikContext())
            {
                if (!context.Users.Any(u => u.EmailUser == student.EmailStudent && u.RoleUser == "Стажер"))
                    return BadRequest("Сначала зарегистрируйтесь");
                if (context.Students.Any(x => x.EmailStudent == student.EmailStudent))
                    return BadRequest("Профиль студента уже создан");
                student.IdStudent = context.Students.Any() ? context.Students.Max(x => x.IdStudent) + 1 : 1;
                student.StatusStudent = "На модерации";
                student.DateCreatedStudent = DateOnly.FromDateTime(DateTime.Now);
                student.TimeCreatedStudent = TimeOnly.FromDateTime(DateTime.Now);
                UserPrice uses = new UserPrice()
                {
                    IdUser = student.IdStudent,
                    IdPrices = 1,
                    Id = context.UserPrices.Any() ? context.UserPrices.Max(x => x.Id) + 1 : 1,
                };
                context.UserPrices.Add(uses);
                context.Students.Add(student);
                context.SaveChanges();

                return Ok(student);
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "UserController.CreateStudent");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        [HttpPut("SaveUserProfile")]
        public ActionResult SaveUserProfile(AllUserprofile? alus)
        {
            try
            {
            if (alus?.student == null)
                return BadRequest("Пустой профиль");
            if (!AuthContext.IsSelfOrAdmin(HttpContext, "student", alus.student.IdStudent))
                return StatusCode(403, "Нет доступа");
            using (var context = new ProfnastavnikContext())
            {
                var existingStudent = context.Students.FirstOrDefault(x => x.IdStudent == alus.student.IdStudent);
                if (existingStudent == null)
                {
                    return NotFound("Пользователь не найден");
                }

                if (alus.student.NameStudent != null)
                    existingStudent.NameStudent = alus.student.NameStudent;

                if (alus.student.SurnameStudent != null)
                    existingStudent.SurnameStudent = alus.student.SurnameStudent;

                if (alus.student.PatronymicStudent != null)
                    existingStudent.PatronymicStudent = alus.student.PatronymicStudent;

                if (alus.student.BirthdayStudent != null)
                    existingStudent.BirthdayStudent = alus.student.BirthdayStudent;

                if (alus.student.CourseStudent.HasValue)
                    existingStudent.CourseStudent = alus.student.CourseStudent;

                if (alus.student.FacultatyStudent != null)
                    existingStudent.FacultatyStudent = alus.student.FacultatyStudent;

                if (alus.student.UniversityStudent.HasValue)
                    existingStudent.UniversityStudent = alus.student.UniversityStudent;

                if (alus.student.EmailStudent != null)
                    existingStudent.EmailStudent = alus.student.EmailStudent;

                if (alus.student.PhotoStudent != null)
                    existingStudent.PhotoStudent = alus.student.PhotoStudent;

                List<UserSkill> allskills = context.UserSkills.Where(x => x.IdUser == alus.student.IdStudent).ToList();
                context.UserSkills.RemoveRange(allskills);

                context.SaveChanges();
                int maxId = context.UserSkills.Any() ? context.UserSkills.Max(x => x.IdUserSkill) : 0;
                foreach (var item in (alus.Skills ?? new List<string>()))
                {
                    maxId++;
                    Skill skilll = context.Skills.FirstOrDefault(x => x.NameSkill == item);
                    if (skilll == null) { maxId--; continue; }
                    UserSkill skill = new UserSkill()
                    {
                        IdSkill = skilll.IdSkill,
                        IdUser = existingStudent.IdStudent,
                        IdUserSkill= maxId,
                    };
                    context.UserSkills.Add(skill);
                }

                List<UserProfession> pr = context.UserProfessions.Where(x => x.IdUser == alus.student.IdStudent).ToList();
                context.UserProfessions.RemoveRange(pr);

                context.SaveChanges();
                int maxId2 = context.UserProfessions.Any() ? context.UserProfessions.Max(x => x.IdUserProfession) : 0;
                foreach (var item in (alus.Proffesions ?? new List<string>()))
                {
                    maxId2++;
                    Profession skilll = context.Professions.FirstOrDefault(x => x.NameProfession == item);
                    if (skilll == null) { maxId2--; continue; }
                    UserProfession skill = new UserProfession()
                    {
                        IdProfession = skilll.IdProfession,
                        IdUser = existingStudent.IdStudent,
                        IdUserProfession = maxId2,
                    };
                    context.UserProfessions.Add(skill);
                }

                List<StudentFile> sf = context.StudentFiles.Where(x => x.IdUser == alus.student.IdStudent).ToList();
                context.StudentFiles.RemoveRange(sf);
                int maxId3 = context.StudentFiles.Any() ? context.StudentFiles.Max(x => x.IdStudentFile) : 0;
                context.SaveChanges();
                foreach (var item in (alus.StudentFiles ?? new List<string>()))
                {
                    maxId3++; 

                    StudentFile skill = new StudentFile()
                    {
                        IdStudentFile = maxId3,
                        IdUser = existingStudent.IdStudent,
                        FileUrl = item,
                    };
                    context.StudentFiles.Add(skill);
                }



                context.SaveChanges();
                return existingStudent != null ? Ok(existingStudent) : NotFound("пользователь не найдены");

            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "UserController.SaveUserProfile");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }

        [HttpPost("AddFaivourites/{id_user}/{id_vacancy}")]
        public ActionResult AddFaivourites(int id_user, int id_vacancy)
        {
            try
            {
                if (!AuthContext.IsSelfOrAdmin(HttpContext, "student", id_user))
                    return StatusCode(403, "Нет доступа");
            using (var context = new ProfnastavnikContext())
            {
                Favourity f = new Favourity()
                {
                    IdFavourity = context.Favourities.Any() ? context.Favourities.Max(x=>x.IdFavourity)+1 : 1,
                    IdVacancy = id_vacancy,
                    IdUser = id_user
                };
                context.Favourities.Add(f);
                context.SaveChanges();
                return Ok(true);
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "UserController.AddFaivourites");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        [HttpDelete("DeleteFaivourites/{id_user}/{id_vacancy}")]
        public ActionResult DeleteFaivourites(int id_user, int id_vacancy)
        {
            try
            {
                if (!AuthContext.IsSelfOrAdmin(HttpContext, "student", id_user))
                    return StatusCode(403, "Нет доступа");
            using (var context = new ProfnastavnikContext())
            {
                Favourity f = context.Favourities.FirstOrDefault(x => x.IdUser==id_user && x.IdVacancy==id_vacancy);
                context.Favourities.Remove(f);
                context.SaveChanges();
                return Ok(true);
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "UserController.DeleteFaivourites");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        [HttpGet("IfFavoutity/{id_user}/{id_vacancy}")]
        public ActionResult IfFavoutity(int id_user, int id_vacancy)
        {
            try
            {
                if (!AuthContext.IsSelfOrAdmin(HttpContext, "student", id_user))
                    return StatusCode(403, "Нет доступа");
            using (var context = new ProfnastavnikContext())
            {
                Favourity f = context.Favourities.FirstOrDefault(x => x.IdUser == id_user && x.IdVacancy == id_vacancy);
                if (f == null)
                {
                    return Ok(false);
                }
                else
                {
                    return Ok(true);
                }
                 
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "UserController.IfFavoutity");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        [HttpGet("GetDirector/{email}")]
        public ActionResult GetDirector(string email)
        {
            try
            {
            using (var context = new ProfnastavnikContext())
            {
                Director user = context.Directors.FirstOrDefault(x => x.EmailDirector == email);
                return user != null ? Ok(user) : NotFound("пользователь не найден");
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "UserController.GetDirector");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        [HttpGet("UserVacancy/{id}")]
        public ActionResult UserVacancy(int id)
        {
            try
            {
            using (var context = new ProfnastavnikContext())
            {
                List<Intership> interships = context.Interships.Where(x => x.IdUser == id && x.StatusIntership == "Идет").Include(x => x.IdCompanyNavigation).Include(x => x.IdVacancyNavigation).ToList();
                var list = interships.Select(x => new UserVacancies
                {
                    NameCompany = x.IdCompanyNavigation.NameCompanyDirector,
                    NameVacancy = x.IdVacancyNavigation.NameVacancy,
                    DateEnd = x.DateEndIntership.HasValue
                    ? (x.DateEndIntership.Value.ToDateTime(TimeOnly.MinValue) - DateTime.Today).TotalDays
                    : 0,
                }).ToList();
                return list.Count()>0 ? Ok(list) : NotFound("вакансии не найдены");
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "UserController.UserVacancy");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        [HttpGet("GetUserMessages/{id}")]
        public ActionResult GetUserMessages(int id)
        {
            try
            {
                if (!AuthContext.IsSelfOrAdmin(HttpContext, "student", id))
                    return StatusCode(403, "Нет доступа");
            using (var context = new ProfnastavnikContext())
            {
                List<Chat> interships = context.Chats.Where(x => x.IdUser == id && (x.SenderChat == "Работодатель" || x.SenderChat=="Админ" ) && x.StatusChat!="Прочитано").ToList();

                return  Ok(interships.Count) ;
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "UserController.GetUserMessages");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }
        [HttpGet("GetUserFeedback/{id}")]
        public ActionResult GetUserFeedback(int id)
        {
            try
            {
            using (var context = new ProfnastavnikContext())
            {
                List<FeedbacksCompany> directory = context.FeedbacksCompanies.Where(x => x.IdUser == id && x.StatusFeedbackCompany!="Удален").ToList();

                return directory.Count() > 0 ? Ok(directory) : NotFound("");
            }
            }
            catch (Exception __ex)
            {
                ApiErrors.Log(__ex, "UserController.GetUserFeedback");
                return StatusCode(500, "Внутренняя ошибка сервера. Попробуйте позже.");
            }
        }

    }
}
