using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ProfNastavnik.Security
{
    /// <summary>Разрешает метод без токена (вход, регистрация, сброс пароля, справочники).</summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class PublicEndpointAttribute : Attribute { }

    /// <summary>
    /// Требует заголовок "Authorization: Bearer {token}". Ставится на контроллер.
    /// Roles = "admin" (или "student,company") ограничивает доступ по роли.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class TokenAuthAttribute : Attribute, IAsyncActionFilter
    {
        public string? Roles { get; set; }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            bool isPublic = context.ActionDescriptor.EndpointMetadata.OfType<PublicEndpointAttribute>().Any();
            if (isPublic)
            {
                await next();
                return;
            }

            string header = context.HttpContext.Request.Headers["Authorization"].ToString();
            string? token = header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? header.Substring(7).Trim() : null;
            var claims = TokenService.Validate(token);
            if (claims == null)
            {
                context.Result = new ObjectResult("Требуется авторизация") { StatusCode = 401 };
                return;
            }

            if (!string.IsNullOrWhiteSpace(Roles))
            {
                var allowed = Roles.Split(',').Select(r => r.Trim().ToLowerInvariant());
                if (!allowed.Contains(claims.Role))
                {
                    context.Result = new ObjectResult("Недостаточно прав") { StatusCode = 403 };
                    return;
                }
            }

            context.HttpContext.Items["auth"] = claims;
            await next();
        }
    }

    /// <summary>Доступ к данным текущего пользователя внутри контроллера.</summary>
    public static class AuthContext
    {
        public static AuthClaims? Get(HttpContext ctx) => ctx.Items["auth"] as AuthClaims;

        /// <summary>true, если запрос делает сам владелец данных (role + id совпали) или администратор.</summary>
        public static bool IsSelfOrAdmin(HttpContext ctx, string role, int id)
        {
            var a = Get(ctx);
            if (a == null) return false;
            if (a.Role == "admin") return true;
            return a.Role == role && a.UserId == id;
        }
    }

    /// <summary>Единое логирование ошибок для try/catch в контроллерах.</summary>
    public static class ApiErrors
    {
        public static void Log(Exception ex, string where)
        {
            Console.WriteLine("[" + DateTime.Now.ToString("HH:mm:ss") + "] Ошибка в " + where + ": " + ex.Message +
                              (ex.InnerException != null ? " | " + ex.InnerException.Message : ""));
        }
    }

    /// <summary>
    /// Подтверждённые кодом email (для сброса пароля). Без этого любой мог бы сменить чужой пароль,
    /// просто отправив запрос ResetPassword.
    /// </summary>
    public static class EmailVerifiedStore
    {
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, DateTime> Items =
            new System.Collections.Concurrent.ConcurrentDictionary<string, DateTime>();

        public static void Mark(string email) => Items[(email ?? "").Trim().ToLowerInvariant()] = DateTime.UtcNow.AddMinutes(15);

        public static bool Consume(string email)
        {
            string key = (email ?? "").Trim().ToLowerInvariant();
            if (Items.TryRemove(key, out var until)) return until > DateTime.UtcNow;
            return false;
        }
    }
}
