using AzureTableIdentity;
using GenericJwtAuth.CryptoService;
using GenericJwtAuth.DTO;
using GenericJwtAuth.Providers;
using GenericJwtAuth.StartupServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GenericJwtAuth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly AzureTableUserManager _userManager;
        private IAzureTableRepo azureTableRepo;
        private readonly Nivra.AzureOperations.Utility authUtility;
        private readonly Nivra.AzureOperations.Utility userTokenUtility;
        private CloudTable authCloudTable;

        public AccountController(
            IAzureTableRepo azureTableRepo
            , Nivra.AzureOperations.Utility authUtility
            , Nivra.AzureOperations.Utility userTokenUtility
            , AzureTableUserManager userManager)
        {
            if (azureTableRepo == null) { throw new ArgumentNullException(nameof(azureTableRepo)); }
            if (authUtility == null) { throw new ArgumentNullException(nameof(authUtility)); }

            this.azureTableRepo = azureTableRepo;
            this.authUtility = authUtility;
            this.userTokenUtility = userTokenUtility;
            this._userManager = userManager;
            var userTwoFactorTokenProvider = new UserTwoFactorTokenProvider(this._userManager, userTokenUtility);
            this._userManager.RegisterTokenProvider("Default", userTwoFactorTokenProvider);
            this._userManager.RegisterTokenProvider("Authenticator", userTwoFactorTokenProvider);
            authCloudTable = this.azureTableRepo.Collection["Auth"];
        }

        [HttpPost("Register")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterAsync(RegisterDto registrationModel, CancellationToken cancellationToken = new CancellationToken())
        {
            if (registrationModel == null) { throw new ArgumentNullException(nameof(registrationModel)); }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //var existingUser = utility.RetrieveEntityUsingPointQuery<AzureTableUser>(AzureTableUser.PARTITIONKEY, registrationModel.Email);

            //if (existingUser != null)
            //{
            //    return BadRequest($"User already exists with email {registrationModel.Email}");
            //}

            AzureTableUser userToInsert = new AzureTableUser()
            {
                Email = registrationModel.Email,
                UserName = registrationModel.Email,
            };

            IdentityResult identityResult = await _userManager.CreateAsync(userToInsert, registrationModel.Password);
            if (identityResult.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(userToInsert.UserName);

                return Ok();
            }
            else
            {
                return BadRequest(identityResult.Errors);
            }
            //await utility.InsertOrMergeEntityAsync<AzureTableUser>(userToInsert);

        }

        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginAsync(LoginDto userModel, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (userModel == null)
            {
                throw new ArgumentNullException(nameof(userModel));
            }
            try
            {
                /* write your logic to compare username and password to that in the database */
                bool loginSuccess = false;
                var userFromDb = await authUtility.RetrieveEntityUsingPointQueryAsync<AzureTableUser>(AzureTableUser.PARTITIONKEY, userModel.NormalizedUserName, cancellationToken)
                    .ConfigureAwait(false);

                if (userFromDb == null)
                {
                    throw new NullReferenceException($"User does not exist with username {userModel.UserName}");
                }

                PasswordVerificationResult passwordVerificationResult = _userManager.PasswordHasher.VerifyHashedPassword(userFromDb, userFromDb.PasswordHash, userModel.Password);


                if (passwordVerificationResult == PasswordVerificationResult.Success)
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
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IActionResult> ForgotPassword(string email)
        {
            AzureTableUser user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound($"User with email {email} is not found");
            }
            string result = await _userManager.GeneratePasswordResetTokenAsync(user);
            // send email


            return Ok(result);
        }

        public async Task<IActionResult> ResetPassword(string email, string resetToken, string newPassword)
        {
            AzureTableUser user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound($"User with email {email} is not found");
            }
            await _userManager.ResetPasswordAsync(user, resetToken, newPassword);
            // send email

            return Ok();
        }

        private Dictionary<string, string> ComposeTokenResponse(string token, AzureTableUser user)
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

            var claims = new List<Claim>() {
                    new Claim("UserId", userModel.Id.ToString()),
                    new Claim(ClaimTypes.Name, userModel.Name),
                    new Claim(ClaimTypes.Email, userModel.Email),
                    // add more claims here
                };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            JwtSecurityTokenHandler securityTokenHandler = new JwtSecurityTokenHandler();

            JwtSecurityToken jwtSecurityToken = new JwtSecurityToken(
                issuer: JwtTokenConfigurations.Issuer,
                audience: JwtTokenConfigurations.Audience,
                claims: claims,
                notBefore: JwtTokenConfigurations.NotBefore,
                expires: JwtTokenConfigurations.ExpiresInMinutes,
                signingCredentials: signingCredentials
                );

            return securityTokenHandler.WriteToken(jwtSecurityToken);
        }

        [Authorize]
        [HttpPost("Restricted")]
        //[Authorize(Roles = "Admin, Manager, SomeBody")]
        //[Authorize(Roles ="NoBody")] //uncommenting this means the user must also belong to role named NoBody
        public IActionResult Restricted()
        {
            return Ok(HttpContext.User.Identity.Name);
        }
    }
}