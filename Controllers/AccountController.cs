using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GenericJwtAuth.CryptoService;
using GenericJwtAuth.DTO;
using GenericJwtAuth.Providers;
using GenericJwtAuth.StartupServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace GenericJwtAuth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private IAzureTableRepo azureTableRepo;
        private Nivra.AzureOperations.Utility utility;
        private CloudTable authCloudTable;

        public AccountController(IAzureTableRepo azureTableRepo, Nivra.AzureOperations.Utility utility)
        {
            if (azureTableRepo == null) { throw new ArgumentNullException(nameof(azureTableRepo)); }
            if (utility == null) { throw new ArgumentNullException(nameof(utility)); }

            this.azureTableRepo = azureTableRepo;
            this.utility = utility;
            authCloudTable = this.azureTableRepo.Collection["Auth"];
        }

        [HttpPost("Register")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterAsync(RegisterDto registrationModel, CancellationToken cancellationToken)
        {
            if (registrationModel == null) { throw new ArgumentNullException(nameof(registrationModel)); }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingUser = utility.RetrieveEntityUsingPointQuery<AzureTableUser>(AzureTableUser.partitionKey, registrationModel.Email);

            if (existingUser != null)
            {
                return BadRequest($"User already exists with email {registrationModel.Email}");
            }

            AzureTableUser userToInsert = new AzureTableUser()
            {
                Email = registrationModel.Email,
                UserName = registrationModel.Email,
                PasswordHash = registrationModel.Password.ToMd5()
            };
            await utility.InsertOrMergeEntityAsync<AzureTableUser>(userToInsert);
            return Ok();

        }

        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginAsync(LoginDto userModel)
        {
            if (userModel == null)
            {
                throw new ArgumentNullException(nameof(userModel));
            }
            try
            {
                /* write your logic to compare username and password to that in the database */
                bool loginSuccess = false;
                var userFromDb = utility.RetrieveEntityUsingPointQuery<AzureTableUser>(AzureTableUser.partitionKey, userModel.UserName);

                loginSuccess = string.Equals(userModel.UserName, userFromDb.NormalizedUserName, StringComparison.InvariantCultureIgnoreCase)
                                && userModel.Password.ToMd5() == userFromDb.PasswordHash;

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
                    var response = ComposeTokenResponse(token, user);
                    return Ok(response);
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

        private Dictionary<string,string> ComposeTokenResponse(string token, AzureTableUser user)
        {
            var dict = new Dictionary<string, string>();
            dict.Add("token", token);
            dict.Add("UserName", user.UserName);
            dict.Add("email", user.Email);
            dict.Add("expires_in", JwtTokenConfigurations.ExpiresInMinutes.Ticks.ToString());
            dict.Add(".expires", JwtTokenConfigurations.ExpiresInMinutes.ToString());

            return dict;
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