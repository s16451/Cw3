using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace APBD
{
    [ApiController]
    [Route("api/login")]
    public class LoginController : ControllerBase
    {
        private readonly IStudentDbService _dbService;
        private readonly IPasswordHashingService _passwordHashingService;
        private readonly IConfiguration _configuration;

        public LoginController(IStudentDbService dbService, IPasswordHashingService passwordHashingService, IConfiguration configuration)
        {
            _configuration = configuration;
            _dbService = dbService;
            _passwordHashingService = passwordHashingService;
        }

        [HttpPost]
        public IActionResult Login(LoginRequest request)
        {
            bool isAuth;
            try
            {
                var credentials = _dbService.GetCredentials(request);
                isAuth = _passwordHashingService.Validate(request.Haslo, credentials.Salt, credentials.Hash);
            }
            catch (Exception e)
            {
                return NotFound(e.Message);
            }

            if (!isAuth)
            {
                return Unauthorized();
            }

            var tokens = GenerateAuthToken("employee");

            try
            {
                _dbService.SaveRefToken(new SaveRefTokenRequest
                {
                    IndexNumber = request.Login,
                    RefToken = tokens.refreshToken.ToString()
                });
            }
            catch (Exception e)
            {
                return NotFound(e.Message);
            }
            
            return Ok(new
            {
                tokens.accessToken,
                tokens.refreshToken
            });
        }

        [HttpPost("refresh-token/{refToken}")]
        public IActionResult RefreshToken(string refToken)
        {
            bool isAuth;
            try
            {
                isAuth = _dbService.IsRefTokenAuth(refToken);
            }
            catch (Exception e)
            {
                return NotFound(e.Message);
            }

            if (!isAuth)
            {
                return Unauthorized();
            }
            
            var tokens = GenerateAuthToken("employee");
            
            try
            {
                _dbService.SaveRefToken(refToken, tokens.refreshToken.ToString());
            }
            catch (Exception e)
            {
                return NotFound(e.Message);
            }
            
            return Ok(new
            {
                tokens.accessToken,
                tokens.refreshToken
            });
        }

        private (string accessToken, Guid refreshToken) GenerateAuthToken(string role)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "user"),
                new Claim(ClaimTypes.Role, role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken
            (
                issuer: "s16451",
                audience: "Students",
                claims: claims,
                expires: DateTime.Now.AddMinutes(20),
                signingCredentials: creds
            );

            return (
                new JwtSecurityTokenHandler().WriteToken(token),
                Guid.NewGuid()
            );
        }
    }
}