using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using ProfNastavnik.Models;

namespace ProfNastavnik.Security
{
    /// <summary>
    /// Настройки версии приложения: appsettings.json -> "App": { "MinVersionCode": 1, "LatestVersionCode": 1, "StoreUrl": "..." }
    /// MinVersionCode: версии приложения ниже этого номера получают ответ 426 и экран «Обновите приложение».
    /// </summary>
    public static class AppRuntime
    {
        private static readonly IConfiguration Cfg = Load();

        private static IConfiguration Load()
        {
            try
            {
                return new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: true)
                    .AddEnvironmentVariables()
                    .Build();
            }
            catch { return new ConfigurationBuilder().Build(); }
        }

        public static int MinVersionCode => int.TryParse(Cfg["App:MinVersionCode"], out var v) ? v : 0;
        public static int LatestVersionCode => int.TryParse(Cfg["App:LatestVersionCode"], out var v) ? v : 1;
        public static string StoreUrl => Cfg["App:StoreUrl"] ?? "";
    }

    /// <summary>Проверка, что аккаунт из токена существует и не заблокирован/удалён (блокировка действует сразу, а не через 30 дней).</summary>
    public static class AccountGuard
    {
        public static bool IsActive(AuthClaims claims)
        {
            if (claims.Role == "admin") return true;
            try
            {
                using (var db = new ProfnastavnikContext())
                {
                    if (claims.Role == "student")
                    {
                        var st = db.Students.Where(x => x.IdStudent == claims.UserId)
                            .Select(x => new { x.EmailStudent, x.StatusStudent }).FirstOrDefault();
                        return st != null && string.Equals(st.EmailStudent, claims.Email, StringComparison.OrdinalIgnoreCase)
                               && (st.StatusStudent == "Активен" || st.StatusStudent == "На модерации");
                    }
                    if (claims.Role == "company")
                    {
                        var d = db.Directors.Where(x => x.IdDirector == claims.UserId)
                            .Select(x => new { x.EmailDirector, x.StatusDirector }).FirstOrDefault();
                        return d != null && string.Equals(d.EmailDirector, claims.Email, StringComparison.OrdinalIgnoreCase)
                               && (d.StatusDirector == "Активен" || d.StatusDirector == "На модерации");
                    }
                }
            }
            catch (Exception ex)
            {
                ApiErrors.Log(ex, "AccountGuard.IsActive");
                return true;  
            }
            return false;
        }
    }

    /// <summary>
    /// Фиксация согласия на обработку персональных данных (кто, когда, какая версия документов, подтверждён ли возраст).
    /// </summary>
    public static class ConsentStore
    {
        public static void Record(ProfnastavnikContext db, string email, string version, string? ip, bool adult)
        {
            try
            {
                db.Database.ExecuteSqlRaw(
                    "INSERT INTO user_consents (email, consent_version, ip, is_adult, accepted_at) VALUES ({0}, {1}, {2}, {3}, {4})",
                    email, version, ip ?? "", adult, DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                ApiErrors.Log(ex, "ConsentStore.Record (выполните create_user_consents.sql)");
            }
        }
    }
}
