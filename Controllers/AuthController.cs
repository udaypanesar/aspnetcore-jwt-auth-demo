using AuthSystemDemo.Dto;
using AuthSystemDemo.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AuthSystemDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IConfiguration configuration) : ControllerBase
    {
        private static User user = new User();

        [HttpPost("AddUser")]
        public ActionResult<User> CreateUser(UserDto requestData)
        {
            if (string.IsNullOrWhiteSpace(requestData.UserName) || string.IsNullOrWhiteSpace(requestData.Password))
            {
                return BadRequest("user name and password is required");
            }
            
            string passwordHash = new PasswordHasher<User>().HashPassword(user,requestData.Password);

            user.UserName = requestData.UserName;
            user.PasswordHash = passwordHash;

            return Ok(user);
        }

        [HttpPost]
        [Route("Login")]
        public ActionResult<string> LoginUser(UserDto requestData)
        {
            if (string.IsNullOrWhiteSpace(requestData.UserName) || string.IsNullOrWhiteSpace(requestData.Password))
            {
                return BadRequest("user name and password is required");
            }

            var verifiedPassword  = new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, requestData.Password);
            
            if (requestData.UserName == user.UserName && PasswordVerificationResult.Success == verifiedPassword)
            {
                return Ok(CreateToken(user));
            }
            else
            {
                return Unauthorized("User login failed");
            }
        }

        private string CreateToken(User user)
        {
            //Add claims
            var claims = new List<Claim>
                            {
                            new Claim(ClaimTypes.Name, user.UserName)
                            };

            //Get secret key
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetValue<string>("AppSettings:Token") ?? ""));

            //Get signing credentials
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            //Create token
            var token = new JwtSecurityToken(issuer: configuration.GetValue<string>("AppSettings:Issuer"),
                                             audience: configuration.GetValue<string>("AppSettings:Audience"),
                                             claims: claims,
                                             expires :DateTime.UtcNow.AddDays(1),
                                             signingCredentials:credentials);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
    }
}
