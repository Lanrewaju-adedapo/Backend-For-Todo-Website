using TestProject.POCO;

namespace TestProject.IRepo
{
    public interface IUserAccessor
    {
        string GetCurrentUserId();
        Task<LoggedInUSer> GetCurrentUserAsync();
    }
}
