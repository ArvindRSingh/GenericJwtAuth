using AzureTableIdentity;
using AzureTableIdentity.Core;
using GenericJwtAuth.Controllers;
using GenericJwtAuth.DTO;
using GenericJwtAuth.StartupServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nivra.AzureOperations;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GenericJwtAuth.Tests
{
    public class Tests
    {
        private readonly AccountController accountController;
        private readonly string _email;
        private readonly string _password;
        private string verificationCode;

        public Tests()
        {
            Startup.Initialize();

            IConfiguration configuration = Startup.GetIConfiguration(TestContext.CurrentContext.TestDirectory);
            JwtTokenConfigurations.Load(configuration);

            Utility authUtility = new Utility(configuration.GetValue<string>("ConnectionStrings:DefaultConnection"), "Auth");
            Utility userTokenUtility = new Utility(configuration.GetValue<string>("ConnectionStrings:DefaultConnection"), "UserTokens");

            AzureTableUserStore<AzureTableUser> azureTableUserStore = new AzureTableUserStore<AzureTableUser>(authUtility.Table, userTokenUtility);
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });

            AzureTableUserManager userManager = new AzureTableUserManager(
                store: new AzureTableUserStore<AzureTableUser>(authUtility.Table, userTokenUtility),
                optionsAccessor: new OptionsAccessor<IdentityOptions>(),
                passwordHasher: new PasswordHasher<AzureTableUser>(),
                userValidators: new List<IUserValidator<AzureTableUser>>() { new UserValidator<AzureTableUser>() },
                passwordValidators: new List<IPasswordValidator<AzureTableUser>>() { new PasswordValidator<AzureTableUser>() },
                keyNormalizer: new LookupNormalizer(),
                errors: new IdentityErrorDescriber(),
                services: null,
                logger: loggerFactory.CreateLogger<UserManager<AzureTableUser>>(),
                textResourceManager: new Nivra.Localization.TextResourceManager(),
                emailSender: new EmailSender(new AuthMessageSenderOptions() { SendGridKey = "", SendGridUser = "" }),
                userTokenUtility: userTokenUtility
                );

            accountController = new Controllers.AccountController(Startup.AzureTableRepo, Startup.AuthUtility, Startup.UserTokenUtility, userManager);

            // 
            _email = Startup.Dict["Email"];
            _password = Startup.Dict["Password"];
        }

        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void Test01RegisterNewUser()
        {
            RegisterDto registerDto = new RegisterDto
            {
                Email = _email,
                Password = _password
            };


            var result = accountController.RegisterAsync(registerDto).Result;
            //Microsoft.AspNetCore.Mvc.IActionResult result = tmpresult.Result;

            Assert.IsNotNull(result);
            Assert.IsTrue(result is OkResult);
            Assert.IsTrue((result as OkResult).StatusCode == 200);

        }

        [Test]
        public void Test02Login()
        {
            LoginDto loginDto = new LoginDto
            {
                UserName = _email,
                Password = _password
            };

            Microsoft.AspNetCore.Mvc.IActionResult result = accountController.LoginAsync(loginDto).Result;

            Assert.IsNotNull(result);
            Assert.IsTrue(result is OkObjectResult);
            Assert.IsTrue((result as OkObjectResult).StatusCode == 200);

        }

        [Test]
        public void Test03ForgotPassword()
        {

            Microsoft.AspNetCore.Mvc.IActionResult result = accountController.ForgotPassword(_email).Result;
            Assert.IsNotNull(result);
            Assert.IsTrue(result is OkObjectResult);
            Assert.IsTrue((result as OkObjectResult).StatusCode == 200);
            this.verificationCode = ((Microsoft.AspNetCore.Mvc.ObjectResult)result).Value.ToString();
        }

        [Test]
        public void Test04ResetPassword()
        {
            Microsoft.AspNetCore.Mvc.IActionResult result = accountController.ResetPassword(_email, verificationCode, _password).Result;
            Assert.IsNotNull(result);
            Assert.IsTrue(result is OkObjectResult || result is OkResult);
            Assert.IsTrue((result as OkObjectResult)?.StatusCode == 200 || (result as OkResult).StatusCode == 200);
        }

        [Test]
        public void Test05ChangeEmail()
        {

        }

        [Test]
        public void Test06ChangeUserName()
        {

        }


    }
}