using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace ProfNastavnik.Security
{

    public static class PasswordHasher
    {
        private const string Prefix = "p1$";
        private const int Iterations = 210_000;
        private const int SaltBytes = 12;
        private const int HashBytes = 24;

        public static string Hash(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltBytes);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashBytes);
            return Prefix + Convert.ToBase64String(salt) + "$" + Convert.ToBase64String(hash);
        }

        public static bool IsModern(string stored) => stored != null && stored.StartsWith(Prefix, StringComparison.Ordinal);

        public static bool NeedsRehash(string stored) => !IsModern(stored);

        public static bool Verify(string password, string stored)
        {
            if (string.IsNullOrEmpty(stored) || password == null) return false;
            try
            {
                if (IsModern(stored))
                {
                    var parts = stored.Substring(Prefix.Length).Split('$');
                    if (parts.Length != 2) return false;
                    byte[] salt = Convert.FromBase64String(parts[0]);
                    byte[] expected = Convert.FromBase64String(parts[1]);
                    byte[] actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, expected.Length);
                    return CryptographicOperations.FixedTimeEquals(expected, actual);
                }
                byte[] legacy = Encoding.UTF8.GetBytes(LegacySha256(password));
                byte[] stor = Encoding.UTF8.GetBytes(stored.Trim().ToLowerInvariant());
                return CryptographicOperations.FixedTimeEquals(legacy, stor);
            }
            catch
            {
                return false;
            }
        }

        private static string LegacySha256(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }

    public static class LoginThrottle
    {
        private const int MaxFails = 5;
        private static readonly TimeSpan LockTime = TimeSpan.FromMinutes(10);
        private static readonly ConcurrentDictionary<string, (int Fails, DateTime LockedUntil)> State =
            new ConcurrentDictionary<string, (int, DateTime)>();

        private static string Key(string email) => (email ?? "").Trim().ToLowerInvariant();

        public static bool IsLocked(string email) =>
            State.TryGetValue(Key(email), out var s) && s.LockedUntil > DateTime.UtcNow;

        public static void Fail(string email)
        {
            State.AddOrUpdate(Key(email), (1, DateTime.MinValue), (_, old) =>
            {
                int fails = old.LockedUntil > DateTime.UtcNow ? old.Fails : old.Fails + 1;
                return fails >= MaxFails ? (0, DateTime.UtcNow.Add(LockTime)) : (fails, old.LockedUntil);
            });
        }

        public static void Reset(string email) => State.TryRemove(Key(email), out _);
    }

    public static class SendThrottle
    {
        private static readonly ConcurrentDictionary<string, DateTime> Last = new ConcurrentDictionary<string, DateTime>();

        public static bool TryAcquire(string email)
        {
            string key = (email ?? "").Trim().ToLowerInvariant();
            var now = DateTime.UtcNow;
            bool allowed = true;
            Last.AddOrUpdate(key, now, (_, prev) =>
            {
                if (now - prev < TimeSpan.FromSeconds(60)) { allowed = false; return prev; }
                return now;
            });
            return allowed;
        }
    }
}
