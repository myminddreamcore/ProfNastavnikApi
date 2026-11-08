using System;
using System.Linq;
using ProfNastavnik.Models;

namespace ProfNastavnik.Security
{
    public class TariffStatus
    {
        public string Role { get; set; } = "";
        public int TariffId { get; set; } = 1;              
        public string TariffName { get; set; } = "";
        public bool IsPaid { get; set; }                    
        public string? ValidUntil { get; set; }             
        public int DaysLeft { get; set; }
        public bool IsExpired { get; set; }                
        public int? ExpiredTariffId { get; set; }
        public string? ExpiredTariffName { get; set; }
        public int DaysOverdue { get; set; }

        public int MaxVacancies { get; set; } = -1;
        public int MaxResponsesPerMonth { get; set; } = -1;
        public bool Chatbot { get; set; }
        public bool PriorityResponses { get; set; }
        public bool TopStudents { get; set; }
        public bool SearchStudents { get; set; }
        public bool InviteStudents { get; set; }

        public int VacanciesUsed { get; set; }
        public int ResponsesUsed { get; set; }

        public string Locked(string feature)
        {
            string s = "«" + feature + "» недоступно на тарифе «" + TariffName + "».";
            if (IsExpired) s += " Срок оплаты тарифа «" + ExpiredTariffName + "» истёк, продлите его.";
            return s;
        }

        public string LimitReached(string what)
        {
            string s = "Лимит тарифа «" + TariffName + "» исчерпан (" + what + ").";
            if (IsExpired) s += " Срок оплаты тарифа «" + ExpiredTariffName + "» истёк, продлите его.";
            return s;
        }
    }

  
    public static class TariffService
    {
        public static TariffStatus Unlimited(string role) => new TariffStatus
        {
            Role = role, TariffId = 99, TariffName = "Администратор", IsPaid = true,
            Chatbot = true, PriorityResponses = true, TopStudents = true, SearchStudents = true, InviteStudents = true
        };

        public static TariffStatus Compute(ProfnastavnikContext ctx, string role, int id)
        {
            var st = new TariffStatus { Role = role };
            var today = DateOnly.FromDateTime(DateTime.Now);
            var monthStart = new DateOnly(today.Year, today.Month, 1);

            int currentId = 1;
            DateOnly? paidAt = null;
            if (role == "student")
            {
                currentId = ctx.UserPrices.Where(x => x.IdUser == id).Select(x => x.IdPrices).FirstOrDefault() ?? 1;
                if (currentId != 1)
                    paidAt = ctx.UserPays.Where(x => x.IdUser == id && x.IdPrice == currentId && x.Date != null)
                        .OrderByDescending(x => x.Date).Select(x => x.Date).FirstOrDefault();
            }
            else
            {
                currentId = ctx.CompanyPrices.Where(x => x.IdCompany == id).Select(x => x.IdPrice).FirstOrDefault() ?? 1;
                if (currentId != 1)
                    paidAt = ctx.CompanyPays.Where(x => x.IdCompany == id && x.IdPay == currentId && x.Date != null)
                        .OrderByDescending(x => x.Date).Select(x => x.Date).FirstOrDefault();
            }

            int effective = 1;
            if (currentId != 1)
            {
                DateOnly? until = paidAt?.AddMonths(1);
                if (until != null && until.Value >= today)
                {
                    effective = currentId;
                    st.IsPaid = true;
                    st.ValidUntil = until.Value.ToString("yyyy-MM-dd");
                    st.DaysLeft = until.Value.DayNumber - today.DayNumber;
                }
                else
                {
                    st.IsExpired = true;
                    st.ExpiredTariffId = currentId;
                    st.ExpiredTariffName = NameOf(ctx, role, currentId);
                    st.DaysOverdue = until == null ? 0 : today.DayNumber - until.Value.DayNumber;
                    if (until != null) st.ValidUntil = until.Value.ToString("yyyy-MM-dd");
                }
            }

            st.TariffId = effective;
            st.TariffName = NameOf(ctx, role, effective);

            if (role == "student")
            {
                st.MaxResponsesPerMonth = effective == 2 ? -1 : 5;
                st.Chatbot = effective == 2;
                st.ResponsesUsed = ctx.Responses.Count(x => x.IdUser == id && x.DateResponse >= monthStart);
            }
            else
            {
                switch (effective)
                {
                    case 3:  
                        st.MaxVacancies = -1; st.MaxResponsesPerMonth = -1;
                        st.Chatbot = st.PriorityResponses = st.TopStudents = st.SearchStudents = st.InviteStudents = true;
                        break;
                    case 2: 
                        st.MaxVacancies = 5; st.MaxResponsesPerMonth = 20;
                        st.Chatbot = st.PriorityResponses = true;
                        break;
                    default:
                        st.MaxVacancies = 1; st.MaxResponsesPerMonth = 5;
                        break;
                }
                st.VacanciesUsed = ctx.Vacancies.Count(x => x.IdCompany == id && (x.StatusVacancy == "Активна" || x.StatusVacancy == "На модерации"));
                st.ResponsesUsed = ctx.Responses.Count(x => x.IdCompany == id && x.DateResponse >= monthStart);
            }
            return st;
        }

        private static string NameOf(ProfnastavnikContext ctx, string role, int tariffId)
        {
            string? name = role == "student"
                ? ctx.PricesForUsers.Where(x => x.IdPrice == tariffId).Select(x => x.NamePrice).FirstOrDefault()
                : ctx.PricesForCompanies.Where(x => x.Id == tariffId).Select(x => x.NamePrice).FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(name)) return name!;
            if (role == "student") return tariffId == 2 ? "Профи" : "Старт";
            return tariffId == 3 ? "Корпоративный" : tariffId == 2 ? "Бизнес" : "Базовый";
        }
    }
}
