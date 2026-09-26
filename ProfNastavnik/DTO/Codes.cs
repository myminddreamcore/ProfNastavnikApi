namespace ProfNastavnik.DTO
{
    public static class VerificationCodeStorage
    {
        private static Dictionary<string, string> _codes = new Dictionary<string, string>();
        private static Dictionary<string, DateTime> _expiry = new Dictionary<string, DateTime>();

        public static void SaveCode(string email, string code)
        {
            _codes[email] = code;
            _expiry[email] = DateTime.Now.AddMinutes(10);
        }

        public static bool VerifyCode(string email, string code)
        {
            if (!_codes.ContainsKey(email)) return false;
            if (_expiry[email] < DateTime.Now) return false;
            return _codes[email] == code;
        }

        public static void RemoveCode(string email)
        {
            if (_codes.ContainsKey(email))
            {
                _codes.Remove(email);
                _expiry.Remove(email);
            }
        }
    }
}
