using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using GenericJwtAuth.Providers;
using GenericJwtAuth.StartupServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace GenericJwtAuth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        [HttpPost("Login")]
        [AllowAnonymous]
        public IActionResult Login(UserModel userModel)
        {
            if (userModel == null)
            {
                throw new ArgumentNullException(nameof(userModel));
            }
            try
            {
                /* write your logic to compare username and password to that in the database */
                bool loginSuccess = true;
                if (loginSuccess)
                {

                    // fetch user from the database instead of using dummy user object below
                    AzureTableUser user = new AzureTableUser()
                    {
                        Email = userModel.UserName,
                        UserName = userModel.UserName,
                        Name = "someName",
                    };
                    var token = GenerateToken(user);
                    return Ok(token);
                }
                else
                {
                    return Unauthorized(userModel);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static string GenerateToken(AzureTableUser userModel)
        {
            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(JwtTokenConfigurations.IssuerSigningKey));
            SigningCredentials signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var roles = new[] { "Admin", "Manager", "Operator", "User" };

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(
                new GenericIdentity(userModel.Email, "token"),
                new List<Claim>() {
                    new Claim("UserId", userModel.Id.ToString()),
                    new Claim(ClaimTypes.Name, userModel.Name),
                    new Claim(ClaimTypes.Email, userModel.Email),
                    // add more claims here
                }
                );

            claimsIdentity.AddClaims(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            JwtSecurityTokenHandler securityTokenHandler = new JwtSecurityTokenHandler();

            SecurityToken securityToken = securityTokenHandler.CreateToken(new SecurityTokenDescriptor()
            {
                Issuer = JwtTokenConfigurations.Issuer,
                Audience = JwtTokenConfigurations.Audience,
                SigningCredentials = signingCredentials,
                Subject = claimsIdentity,
                Expires = JwtTokenConfigurations.ExpiresInMinutes,
                NotBefore = JwtTokenConfigurations.NotBefore,


            });

            return securityTokenHandler.WriteToken(securityToken);
        }

        [HttpGet("Restricted")]
        [Authorize(Roles = "Admin, Manager, SomeBody")]
        //[Authorize(Roles ="NoBody")] //uncommenting this means the user must also belong to role named NoBody
        public IActionResult Restricted()
        {
            return Ok(HttpContext.User.Identity.Name);
        }
    }
}