using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TestProject.Data;
using TestProject.IRepo;
using TestProject.Models.Authentication;
using TestProject.POCO;

namespace TestProject.Services.Helpers
{
    public class UserAccessor : IUserAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly TodoAppContext _context;

        public UserAccessor(IHttpContextAccessor httpContextAccessor, TodoAppContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        public string GetCurrentUserId()
        {
            var userId = _httpContextAccessor.HttpContext?.User?
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return userId;
        }

        public async Task<LoggedInUSer> GetCurrentUserAsync()
        {
            var userId = GetCurrentUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return null;
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id.ToString() == userId);

            return new LoggedInUSer
            {
                UserId = user.Id.ToString(),
            };
        }
    }
}
