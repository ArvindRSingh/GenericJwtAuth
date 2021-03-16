using AzureTableIdentity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nivra.AzureOperations;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GenericJwtAuth.Providers
{
    public class PasswordResetTokenProvider<TUser>
        : DataProtectorTokenProvider<TUser>
        where TUser : class
    {
        public PasswordResetTokenProvider(IDataProtectionProvider dataProtectionProvider,
            IOptions<PasswordResetTokenProviderOptions> options,
            ILogger<DataProtectorTokenProvider<TUser>> logger)
            : base(dataProtectionProvider, options, logger)
        {

        }
    }

    public class PasswordResetTokenProviderOptions : DataProtectionTokenProviderOptions
    {
        public PasswordResetTokenProviderOptions()
        {
            Name = "PasswordResetTokenProvider";
            TokenLifespan = TimeSpan.FromDays(3);
        }
    }

    public class ChangePhoneNumberTokenProvider<TUser> : Microsoft.AspNetCore.Identity.TotpSecurityStampBasedTokenProvider<TUser>
 where TUser : class
    {
        public ChangePhoneNumberTokenProvider() : base()
        {

        }
        public override Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<TUser> manager, TUser user)
        {
            return Task.FromResult(true);
        }
    }

    public class EmailTokenProvider<TUser>
        : Microsoft.AspNetCore.Identity.EmailTokenProvider<TUser>
        where TUser : class
    {
        public EmailTokenProvider() : base()
        {

        }
    }

    public class UserTwoFactorTokenProvider
        : IUserTwoFactorTokenProvider<AzureTableUser>
    {
        private readonly AzureTableUserManager userManager;

        public Utility UserTokenUtility { get; }

        public UserTwoFactorTokenProvider(AzureTableUserManager userManager, Utility userTokenUtility)
        {
            this.userManager = userManager;
            this.UserTokenUtility = userTokenUtility;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="purpose">Valid values: Authentication, ResetPassword, ChangeEmail, ChangePhoneNumber and  EmailConfirmation.</param>
        /// 
        /// <param name="manager"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        /// <seealso cref="ValidateAsync(string, string, UserManager{TUser}, TUser)"/>
        public async Task<string> GenerateAsync(string purpose, UserManager<AzureTableUser> manager, AzureTableUser user)
        {
            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            string token;
            switch (purpose)
            {
                case "Authentication":
                    throw new NotImplementedException("Generate authentication token not implemented.");
                case "ResetPassword":
                    return await this.GenerateTokenAsync(user, purpose).ConfigureAwait(false);
                case "ChangeEmail":
                    return await manager.GenerateEmailConfirmationTokenAsync(user).ConfigureAwait(false);
                case "ChangePhoneNumber":
                    return await manager.GenerateChangePhoneNumberTokenAsync(user, user.PhoneNumber).ConfigureAwait(false);
                case "EmailConfirmation":
                    return await this.GenerateTokenAsync(user, purpose).ConfigureAwait(false);
                default:
                    throw new ArgumentException(nameof(purpose));
            }
            throw new ArgumentException(nameof(purpose));
        }

        private Task<string> GenerateTokenAsync(AzureTableUser user, string purpose)
        {
#pragma warning disable CA1305 // Specify IFormatProvider
            string token = new Random().Next(100000, 999999).ToString();
#pragma warning restore CA1305 // Specify IFormatProvider

            return Task.FromResult(token);
        }

        public Task<bool> ValidateAsync(string purpose, string token, UserManager<AzureTableUser> manager, AzureTableUser user)
        {
            return this.ValidateAsync(purpose, token, manager, user, default(CancellationToken));
        }

        /// <summary>
        /// Valid values: Authentication, ResetPassword, ChangeEmail, ChangePhoneNumber and  EmailConfirmation.
        /// </summary>
        /// <param name="purpose"></param>
        /// <param name="token"></param>
        /// <param name="manager"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        /// <seealso cref="GenerateAsync(string, UserManager{TUser}, TUser)"/>
        public Task<bool> ValidateAsync(string purpose, string token, UserManager<AzureTableUser> manager, AzureTableUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }


            switch (purpose)
            {
                case "Authentication":
                case "TwoFactor":
                    return VerifyTwoFactorTokenAsync(user, manager.Options.Tokens.AuthenticatorTokenProvider, token, cancellationToken);
                case "ResetPassword":
                    return manager.VerifyTwoFactorTokenAsync(user, manager.Options.Tokens.PasswordResetTokenProvider, token);
                case "ChangeEmail":
                    return manager.VerifyTwoFactorTokenAsync(user, manager.Options.Tokens.AuthenticatorTokenProvider, token);
                case "ChangePhoneNumber":
                    return manager.VerifyChangePhoneNumberTokenAsync(user, token, user.PhoneNumber);
                case "EmailConfirmation":
                    return manager.VerifyUserTokenAsync(user, manager.Options.Tokens.EmailConfirmationTokenProvider, "EmailConfirmation", token);
                default:
                    throw new ArgumentException($"Could not find a provider for {nameof(purpose)}.");
            }
        }

        private async Task<bool> VerifyTwoFactorTokenAsync(AzureTableUser user, string authenticatorTokenProvider, string token, CancellationToken cancellationToken = default(CancellationToken))
        {
            TableOperation operation = TableOperation.Retrieve<TokenEntity>("UserTokens", user.UserName);
            var result = await this.UserTokenUtility.RetrieveEntityUsingPointQueryAsync<TokenEntity>("UserTokens", user.UserName, cancellationToken)
                            .ConfigureAwait(false);

            return string.Compare(token, result.Token, StringComparison.InvariantCulture) == 0;
        }

        public Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<AzureTableUser> manager, AzureTableUser user)
        {

            return Task.FromResult(true);

        }
    }
}