using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfNastavnik.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ProfNastavnik.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestController : ControllerBase
    {
        [HttpGet("GetUserRequests/{userId}")]
        public async Task<ActionResult<List<RequestsUserDTO>>> GetUserRequests(int userId)
        {
            try
            {
                using var context = new ProfnastavnikContext();

                var requests = await context.RequestsUsers
                    .Include(r => r.IdStudentNavigation)
                    .Include(r => r.IdFeedbackNavigation)
                    .Where(r => r.IdStudent == userId)
                    .Select(r => new RequestsUserDTO
                    {
                        IdRequest = r.IdRequest,
                        IdStudent = r.IdStudent,
                        IdFeedback = r.IdFeedback,
                        DescriptionRequest = r.DescriptionRequest,
                        StatusRequest = r.StatusRequest,
                        DateRequest = r.DateRequest.HasValue ? r.DateRequest.Value.ToString("dd.MM.yyyy") : null,
                        StudentName = r.IdStudentNavigation != null ?
                            $"{r.IdStudentNavigation.SurnameStudent} {r.IdStudentNavigation.NameStudent}" : null,
                        FeedbackText = r.IdFeedbackNavigation != null ?
                            r.IdFeedbackNavigation.DescriptionCompany : null
                    })
                    .ToListAsync();

                return Ok(requests);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка: {ex.Message}");
            }
        }

        [HttpGet("GetCompanyRequests/{companyId}")]
        public async Task<ActionResult<List<RequestsCompanyDTO>>> GetCompanyRequests(int companyId)
        {
            try
            {
                using var context = new ProfnastavnikContext();

                var requests = await context.RequestsCompanies
                    .Include(r => r.IdCompanyNavigation)
                    .Include(r => r.IdFeedbackNavigation)
                    .Where(r => r.IdCompany == companyId)
                    .Select(r => new RequestsCompanyDTO
                    {
                        IdRequest = r.IdRequest,
                        IdCompany = r.IdCompany,
                        IdFeedback = r.IdFeedback,
                        DescriptionRequset = r.DescriptionRequset,
                        StatusFeedback = r.StatusFeedback,
                        DateRequest = r.DateRequest.HasValue ? r.DateRequest.Value.ToString("dd.MM.yyyy") : null,
                        CompanyName = r.IdCompanyNavigation != null ?
                            r.IdCompanyNavigation.NameCompanyDirector : null,
                        FeedbackText = r.IdFeedbackNavigation != null ?
                            r.IdFeedbackNavigation.DescriptionFeedbackUser : null
                    })
                    .ToListAsync();

                return Ok(requests);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка: {ex.Message}");
            }
        }
        [HttpPost("CreateUserRequest")]
        public async Task<ActionResult> CreateUserRequest([FromBody] CreateRequestUserDTO request)
        {
            try
            {
                using var context = new ProfnastavnikContext();
                var newRequest = new RequestsUser
                {
                    IdRequest = context.RequestsUsers.Any() ? context.RequestsUsers.Max(x => x.IdRequest) + 1 : 1,
                    IdStudent = request.IdStudent,
                    IdFeedback = request.IdFeedback != 0 ? request.IdFeedback : null,
                    DescriptionRequest = request.DescriptionRequest,
                    StatusRequest = "На модерации",
                    DateRequest = DateOnly.FromDateTime(DateTime.UtcNow)
                };
                RequestsUser current = context.RequestsUsers.FirstOrDefault(x=>x.IdStudent == newRequest.IdStudent && x.IdFeedback==newRequest.IdFeedback);
                if (current != null)
                {
                    return BadRequest("такая жалоба уже есть");
                }
                

                context.RequestsUsers.Add(newRequest);
                await context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка: {ex.Message}");
            }
        }

        [HttpPost("CreateCompanyRequest")]
        public async Task<ActionResult> CreateCompanyRequest([FromBody] CreateRequestCompanyDTO request)
        {
            try
            {
                using var context = new ProfnastavnikContext();



                var newRequest = new RequestsCompany
                {
                    IdRequest = context.RequestsCompanies.Any() ? context.RequestsCompanies.Max(x => x.IdRequest) + 1 : 1,
                    IdCompany = request.IdCompany,
                    IdFeedback = request.IdFeedback != 0 ? request.IdFeedback : null,
                    DescriptionRequset = request.DescriptionRequset,
                    StatusFeedback = "На модерации",
                    DateRequest = DateOnly.FromDateTime(DateTime.UtcNow)
                };
                RequestsCompany current = context.RequestsCompanies.FirstOrDefault(x => x.IdCompany == newRequest.IdCompany && x.IdFeedback == newRequest.IdFeedback);
                if (current != null)
                {
                    return BadRequest("такая жалоба уже есть");
                }
                context.RequestsCompanies.Add(newRequest);
                await context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");
                return StatusCode(500, $"Ошибка: {ex.Message}");
            }
        }
      
        public class DirectUserRequest
        {
            public int IdStudent { get; set; }
            public int TargetUserId { get; set; }
            public string DescriptionRequest { get; set; }
        }

        public class DirectCompanyRequest
        {
            public int IdCompany { get; set; }
            public int TargetCompanyId { get; set; }
            public string DescriptionRequset { get; set; }
        }

        public class RequestsUserDTO
        {
            public int IdRequest { get; set; }
            public int? IdStudent { get; set; }
            public int? IdFeedback { get; set; }
            public string DescriptionRequest { get; set; }
            public string StatusRequest { get; set; }
            public string DateRequest { get; set; }
            public string StudentName { get; set; }
            public string FeedbackText { get; set; }
        }

        public class RequestsCompanyDTO
        {
            public int IdRequest { get; set; }
            public int? IdCompany { get; set; }
            public int? IdFeedback { get; set; }
            public string DescriptionRequset { get; set; }
            public string StatusFeedback { get; set; }
            public string DateRequest { get; set; }
            public string CompanyName { get; set; }
            public string FeedbackText { get; set; }
        }

        public class CreateRequestUserDTO
        {
            public int IdStudent { get; set; }
            public int IdFeedback { get; set; }
            public string DescriptionRequest { get; set; }
        }

        public class CreateRequestCompanyDTO
        {
            public int IdCompany { get; set; }
            public int IdFeedback { get; set; }
            public string DescriptionRequset { get; set; }
        }
    }
}

