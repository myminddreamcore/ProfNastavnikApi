using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ProfNastavnik.Security
{
    /// <summary>Данные, зашитые в токен.</summary>
    public class AuthClaims
    {
        public int UserId { get; set; }          // IdStudent / IdDirector / 1 для администратора
        public string Email { get; set; } = "";
        public string Role { get; set; } = "";   // "student" | "company" | "admin"
        public long Exp { get; set; }            // unix-время окончания, сек
    }

    /// <summary>
    /// Подписанные токены (HMAC-SHA256), без БД и без доп. пакетов: "v1.{payload}.{signature}".
    /// Секрет берётся из переменной окружения AUTH_SECRET или appsettings.json -> "Auth:Secret".
    /// Если секрет не задан, генерируется случайный на время работы процесса
    /// (после перезапуска сервера все пользователи просто войдут заново).
    /// </summary>
    public static class TokenService
    {
        private static readonly byte[] Key = LoadKey();
        private static readonly TimeSpan Ttl = TimeSpan.FromDays(30);

        private static byte[] LoadKey()
        {
            string? secret = Environment.GetEnvironmentVariable("AUTH_SECRET");
            if (string.IsNullOrWhiteSpace(secret))
            {
                try
                {
                    var cfg = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                        .SetBasePath(AppContext.BaseDirectory)
                        .AddJsonFile("appsettings.json", optional: true)
                        .Build();
                    secret = cfg["Auth:Secret"];
                }
                catch { /* игнорируем, возьмём случайный */ }
            }
            if (string.IsNullOrWhiteSpace(secret) || secret.Length < 16)
            {
                Console.WriteLine("[Auth] ВНИМАНИЕ: Auth:Secret не задан (или короче 16 символов). Используется временный ключ: токены перестанут действовать после перезапуска.");
                return RandomNumberGenerator.GetBytes(32);
            }
            return Encoding.UTF8.GetBytes(secret);
        }

        private static string B64(byte[] data) =>
            Convert.ToBase64String(data).TrimEnd('=').Replace('+', '-').Replace('/', '_');

        private static byte[] FromB64(string s)
        {
            s = s.Replace('-', '+').Replace('_', '/');
            switch (s.Length % 4) { case 2: s += "=="; break; case 3: s += "="; break; }
            return Convert.FromBase64String(s);
        }

        private static byte[] Sign(string data)
        {
            using (var h = new HMACSHA256(Key))
                return h.ComputeHash(Encoding.UTF8.GetBytes(data));
        }

        public static string Issue(int userId, string email, string role)
        {
            var claims = new AuthClaims
            {
                UserId = userId,
                Email = email ?? "",
                Role = role,
                Exp = DateTimeOffset.UtcNow.Add(Ttl).ToUnixTimeSeconds()
            };
            string payload = B64(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(claims)));
            string body = "v1." + payload;
            return body + "." + B64(Sign(body));
        }

        /// <summary>Возвращает claims или null, если токен подделан/просрочен/некорректен.</summary>
        public static AuthClaims? Validate(string? token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token)) return null;
                var parts = token.Split('.');
                if (parts.Length != 3 || parts[0] != "v1") return null;

                string body = parts[0] + "." + parts[1];
                byte[] expected = Sign(body);
                byte[] actual = FromB64(parts[2]);
                if (!CryptographicOperations.FixedTimeEquals(expected, actual)) return null;

                var claims = JsonSerializer.Deserialize<AuthClaims>(Encoding.UTF8.GetString(FromB64(parts[1])));
                if (claims == null) return null;
                if (claims.Exp < DateTimeOffset.UtcNow.ToUnixTimeSeconds()) return null;
                return claims;
            }
            catch
            {
                return null;
            }
        }
    }
}
