using TestProject.POCO;
using TestProject.ViewModels_DTOs_;

namespace TestProject.IRepo
{
    public interface IAuthorizationService
    {
        Task<StatusMessage> RegisterUser(RegisterUserDTO userObj);
        Task<LoginResponse> LoginUser(UserLogin userObj);
        Task<LoginResponse> RefreshToken (string token, string refreshToken);
    }
}
