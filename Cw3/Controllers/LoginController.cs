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
        private const string ConString = "Data Source=db-mssql;Initial Catalog=s16451;Integrated Security=True";
        private readonly IStudentDbService _service;
        private readonly IConfiguration _configuration;

        public LoginController(IStudentDbService service, IConfiguration configuration)
        {
            _configuration = configuration;
            _service = service;
        }
        
        [HttpPost]
        public IActionResult Login(LoginRequest request)
        {
            bool isAuth;
            try
            {
                isAuth = _service.IsAuthStudent(request);
            }
            catch (Exception e)
            {
                return NotFound(e.Message);
            }

            if (!isAuth)
            {
                return Unauthorized();
            }
            
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "user"),
                new Claim(ClaimTypes.Role, "employee")
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

            return Ok(new
            {
                accessToken = new JwtSecurityTokenHandler().WriteToken(token),
                refreshToken=Guid.NewGuid()
            });
        } 
    }
}