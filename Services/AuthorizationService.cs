using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TestProject.IRepo;
using TestProject.Models.Authentication;
using TestProject.POCO;
using TestProject.ViewModels_DTOs_;

namespace TestProject.Services
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly UserManager<Users> _userManager;
        private readonly SignInManager<Users> _signInManager;
        private readonly IConfiguration _configuration;

        public AuthorizationService(UserManager<Users> userManager, SignInManager<Users> signInManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }
        public async Task<StatusMessage> RegisterUser(RegisterUserDTO userObj)
        {
            try
            {
                var emailExists = await _userManager.FindByEmailAsync(userObj.Email);

                if (emailExists != null)
                {
                    return new StatusMessage
                    {
                        Status = "Failed",
                        Message = $"email {userObj.Email} already in Use"
                    };
                }

                if (userObj.Password != userObj.ConfirmPassword)
                {
                    return new StatusMessage
                    {
                        Status = "Failed",
                        Message = "Passwords Do not Match"
                    };
                }

                var newUser = new Users
                {
                    UserName = userObj.UserName,
                    Email = userObj.Email,
                    FirstName = userObj.FirstName,
                    LastName = userObj.LastName,
                };

                //var ttlDays = _configuration.GetValue<int?>("Jwt:RefreshTokenDays") ?? 7;
                //newUser.RefreshToken = GenerateRefreshToken();
                //newUser.RefreshTokenExpiry = DateTime.UtcNow.AddDays(ttlDays);

                var result = await _userManager.CreateAsync(newUser, userObj.Password);

                var errors = string.Join(", ", result.Errors.Select(e => e.Description));

                if (!result.Succeeded)
                {
                    return new StatusMessage
                    {
                        Status = "Failed",
                        Message = $"Failed to create User: {errors}"
                    };
                }



                //var updateResult = await _userManager.UpdateAsync(newUser);
                //if (!updateResult.Succeeded)
                //{
                //    // If update fails, decide whether to treat as fatal or log and continue.
                //    var updateErrors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                //    return new StatusMessage
                //    {
                //        Status = "Failed",
                //        Message = $"User created but failed to set refresh token: {updateErrors}"
                //    };
                //}

                return new StatusMessage
                {
                    Status = "Success",
                    Message = $"User {userObj.Email} added successfully",
                    data = userObj
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<LoginResponse> LoginUser(UserLogin userObj)
        {
            var userExists = await _userManager.FindByEmailAsync(userObj.Email);

            if (userExists == null)
            {
                return new LoginResponse
                {
                    Status = "Failed",
                    Message = "Email Not Found"
                };
            }

            var result = await _signInManager.CheckPasswordSignInAsync(userExists, userObj.Password, false);

            if (!result.Succeeded)
            {
                return new LoginResponse
                {
                    Status = "Failed",
                    Message = "Login Attempt Failed Invalid password"
                };
            }

            var token = GenerateJwtToken(userExists);
            var refreshToken = GenerateRefreshToken();

            userExists.RefreshToken = refreshToken;
            userExists.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(userExists);

            return new LoginResponse
            {
                UserInfo = new UserInfoDto
                {
                    UserName = userExists.UserName,
                    Email = userExists.Email,
                    FirstName = userExists.FirstName,
                    LastName = userExists.LastName,
                },
                Status = "Success",
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = refreshToken,
                RefreshTokenExpiry = token.ValidTo
            };
        }

        private JwtSecurityToken GenerateJwtToken(Users user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            return new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpirationInMinutes"])),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256)
            );
        }

        public async Task<LoginResponse> RefreshToken(string token, string refreshToken)
        {
            try
            {
                var principal = GetPrincipalFromExpiredToken(token);
                var UserId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(UserId))
                {
                    return new LoginResponse
                    {
                        Status = "Failed",
                        Message = "Invalid Token"
                    };
                }

                var user = await _userManager.FindByIdAsync(UserId);
                if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiry <= DateTime.UtcNow)
                {
                    return new LoginResponse { Status = "Failed", Message = "Invalid refresh token" };
                }

                var newJwtToken = GenerateJwtToken(user);
                var newRefreshToken = GenerateRefreshToken();

                user.RefreshToken = newRefreshToken;
                user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
                await _userManager.UpdateAsync(user);

                return new LoginResponse
                {
                    Status = "Success",
                    Token = new JwtSecurityTokenHandler().WriteToken(newJwtToken),
                    RefreshToken = newRefreshToken,
                    RefreshTokenExpiry = newJwtToken.ValidTo,
                    UserInfo = new UserInfoDto
                    {
                        UserName = user.UserName,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                    },
                };
            }
            catch (SecurityTokenException)
            {
                return new LoginResponse { Status = "Failed", Message = "Invalid token" };
            }
            catch (Exception ex)
            {
                return new LoginResponse { Status = "Failed", Message = ex.Message };
            }

        }


        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            try
            {

                var jwtSettings = _configuration.GetSection("JwtSettings");
                var secretKey = jwtSettings["SecretKey"];

                if (string.IsNullOrEmpty(secretKey))
                {
                    throw new ArgumentException("JWT SecretKey is not configured");
                }

                var key = Encoding.UTF8.GetBytes(secretKey);

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,    // Don't validate audience for expired tokens
                    ValidateIssuer = false,      // Don't validate issuer for expired tokens
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateLifetime = false,     // MOST IMPORTANT: Don't validate lifetime!
                    ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 }
                };

                var tokenHandler = new JwtSecurityTokenHandler();

                // Check if token can be read
                if (!tokenHandler.CanReadToken(token))
                {
                    throw new SecurityTokenException("Invalid token format");
                }

                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);

                if (validatedToken is not JwtSecurityToken jwtSecurityToken)
                {
                    throw new SecurityTokenException("Invalid token type");
                }

                if (!jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new SecurityTokenException("Invalid token algorithm");
                }
                return principal;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        public class UserInfoDto
        {
            public string? UserName { get; set; }
            public string? Email { get; set; }
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
        }
    }
}

