using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using TestProject.IRepo;
using TestProject.ViewModels_DTOs_;

namespace TestProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly TestProject.IRepo.IAuthorizationService _authorizationService;

        public AuthController(TestProject.IRepo.IAuthorizationService authorizationService) 
        {
            _authorizationService = authorizationService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUserDTO registerUser)
        {
            var result = await _authorizationService.RegisterUser(registerUser);

            if (result?.Status?.ToLower() == "failed")
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> LoginUser([FromBody] UserLogin loggedInUser)
        {
            var result = await _authorizationService.LoginUser(loggedInUser);

            if (result?.Status?.ToLower() == "failed")
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("refreshToken")]
        public async Task<IActionResult> RefreshToken(string token, string RefreshToken)
        {
            var result = await _authorizationService.RefreshToken(token, RefreshToken);

            if (result.Status.ToLower() == "failed")
            {
                return Unauthorized(result.Message);
            }

            return Ok(new
            {
                token = result.Token,
                refreshToken = result.RefreshToken,
                expiration = result.RefreshTokenExpiry,
                user = result.UserInfo
            });
        }
    }
}
