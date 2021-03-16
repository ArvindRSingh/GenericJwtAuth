using AzureTableIdentity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace AzureTableIdentity.Core.Providers
{
    public class PasswordResetTokenProvider<TUser>
        : DataProtectorTokenProvider<TUser>
        where TUser : class
    {
        public PasswordResetTokenProvider(IDataProtectionProvider dataProtectionProvider,
            IOptions<PasswordResetTokenProviderOptions> options,
            ILogger<DataProtectorTokenProvider<TUser>> logger)
            : base(dataProtectionProvider, options)
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
            string token;
            switch (purpose)
            {
                case "Authentication":
                    throw new NotImplementedException("Generate authentication token not implemented.");
                case "ResetPassword":
                    return await manager.GeneratePasswordResetTokenAsync(user);
                case "ChangeEmail":
                    return await manager.GenerateEmailConfirmationTokenAsync(user);
                case "ChangePhoneNumber":
                    return await manager.GenerateChangePhoneNumberTokenAsync(user, user.PhoneNumber);
                case "EmailConfirmation":
                    return await manager.GenerateEmailConfirmationTokenAsync(user);
                default:
                    throw new ArgumentException(nameof(purpose));
            }
            throw new ArgumentException(nameof(purpose));
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
        public Task<bool> ValidateAsync(string purpose, string token, UserManager<AzureTableUser> manager, AzureTableUser user)
        {
            switch (purpose)
            {
                case "Authentication":
                    return manager.VerifyTwoFactorTokenAsync(user, manager.Options.Tokens.AuthenticatorTokenProvider, token);
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

        public Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<AzureTableUser> manager, AzureTableUser user)
        {

            return Task.FromResult(true);

        }
    }
}