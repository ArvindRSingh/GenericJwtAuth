using AzureTableIdentity.Core;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nivra.AzureOperations;
using Nivra.Localization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace AzureTableIdentity
{
    public class AzureTableUserManager : UserManager<AzureTableUser>
    {
        private readonly IEmailSender emailSender;

        public Utility UserTokenUtility { get; }

        private TextResourceManager textResourceManager;

        public AzureTableUserManager(
            AzureTableUserStore<AzureTableUser> store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<AzureTableUser> passwordHasher,
            IEnumerable<IUserValidator<AzureTableUser>> userValidators,
            IEnumerable<IPasswordValidator<AzureTableUser>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager<AzureTableUser>> logger,
            TextResourceManager textResourceManager,
            IEmailSender emailSender,
            Utility userTokenUtility)
            : base(store
                  , optionsAccessor
                  , passwordHasher
                  , userValidators
                  , passwordValidators
                  , keyNormalizer
                  , errors
                  , services
                  , logger
                  )
        {
            base.RegisterTokenProvider(
                TokenOptions.DefaultPhoneProvider,
                new PhoneNumberTokenProvider<AzureTableUser>()
                );

            this.textResourceManager = textResourceManager;
            this.emailSender = emailSender;
            this.UserTokenUtility = userTokenUtility;
        }

        public override Task<IdentityResult> CreateAsync(AzureTableUser user)
        {
            return base.CreateAsync(user);
        }
        public override async Task<IdentityResult> CreateAsync(AzureTableUser user, string password)
        {
            var code = await base.GenerateEmailConfirmationTokenAsync(user);
            await emailSender.SendEmailAsync(GenericJwtAuth.DTO.Constants.EmailSender, user.Email, "", "");
            return await base.CreateAsync(user, password);
        }
        public override async Task<string> GeneratePasswordResetTokenAsync(AzureTableUser user)
        {
            var tmpUser = await this.FindByNameAsync(user.Email);
            string code = await base.GeneratePasswordResetTokenAsync(user);
            var result = await this.SetAuthenticationTokenAsync(user, "Default", TokenTypeEnum.PasswordResetToken.ToString(), code);
            await emailSender.SendEmailAsync(GenericJwtAuth.DTO.Constants.EmailSender, user.Email, "", "");
            return code;
        }

        private async Task<IdentityResult> UpdateUserTokensAsync(AzureTableUser user, string tokenValue)
        {
            if (user == null) { throw new ArgumentNullException(nameof(user)); }
            var tokenObject = new TokenEntity(user, tokenValue) { Provider = "" };
            TableOperation executeOperation = TableOperation.InsertOrMerge(tokenObject);

            try
            {
                await UserTokenUtility.Table.ExecuteAsync(executeOperation);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
            return IdentityResult.Success;
        }

        //public override async Task<IdentityResult> ResetPasswordAsync(AzureTableUser user, string token, string newPassword)
        //{
        //    var result = await this.VerifyTwoFactorTokenAsync(user, "Default", token);

        //    return await base.ResetPasswordAsync(user, token, newPassword);
        //}


        public override async Task<IdentityResult> SetAuthenticationTokenAsync(AzureTableUser user, string loginProvider, string tokenName, string tokenValue)
        {
            await UpdateUserTokensAsync(user, tokenValue);
            return await base.SetAuthenticationTokenAsync(user, loginProvider, tokenName, tokenValue);
        }
    }
}