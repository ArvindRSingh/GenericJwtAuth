using AzureTableIdentity;
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
        public Tests()
        {
            Startup.Initialize();

            IConfiguration configuration = Startup.GetIConfiguration(TestContext.CurrentContext.TestDirectory);
            JwtTokenConfigurations.Load(configuration);

            var utility = new Utility(configuration.GetValue<string>("ConnectionStrings:DefaultConnection"), "Auth");

            AzureTableUserStore<AzureTableUser> azureTableUserStore = new AzureTableUserStore<AzureTableUser>(utility.Table);
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });

            UserManager<AzureTableUser> userManager = new UserManager<AzureTableUser>(
                store: new AzureTableUserStore<AzureTableUser>(utility.Table),
                optionsAccessor: new OptionsAccessor<IdentityOptions>(),
                passwordHasher: new PasswordHasher<AzureTableUser>(),
                userValidators: new List<IUserValidator<AzureTableUser>>() { new UserValidator<AzureTableUser>() },
                passwordValidators: new List<IPasswordValidator<AzureTableUser>>() { new PasswordValidator<AzureTableUser>() },
                keyNormalizer: new LookupNormalizer(),
                errors: new IdentityErrorDescriber(),
                services: null,
                logger: loggerFactory.CreateLogger<UserManager<AzureTableUser>>()
                );

            accountController = new Controllers.AccountController(Startup.AzureTableRepo, Startup.Utility, userManager);
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
                Email = Startup.Dict["Email"],
                Password = Startup.Dict["Password"]
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
                UserName = Startup.Dict["Email"],
                Password = Startup.Dict["Password"]
            };

            Microsoft.AspNetCore.Mvc.IActionResult result = accountController.LoginAsync(loginDto).Result;

            Assert.IsNotNull(result);
            Assert.IsTrue(result is OkObjectResult);
            Assert.IsTrue((result as OkObjectResult).StatusCode == 200);

            Assert.Pass();
        }

        [Test]
        public void Test03ChangePassword()
        {

        }

        [Test]
        public void Test04ForgotPassword()
        {

        }

        [Test]
        public void Test04ChangeEmail()
        {

        }

        [Test]
        public void Test05ChangeUserName()
        {

        }


    }
}