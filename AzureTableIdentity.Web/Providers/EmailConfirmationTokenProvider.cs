using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace GenericJwtAuth.Providers
{
    public class AppEmailConfirmationTokenProvider<TUser>: Microsoft.AspNetCore.Identity.EmailTokenProvider<TUser>
        where TUser:class
    {
        public override Task<bool> ValidateAsync(string purpose, string token, UserManager<TUser> manager, TUser user)
        {
            return base.ValidateAsync(purpose, token, manager, user);
        }
        public override Task<string> GetUserModifierAsync(string purpose, UserManager<TUser> manager, TUser user)
        {
            return base.GetUserModifierAsync(purpose, manager, user);
        }
        public override Task<string> GenerateAsync(string purpose, UserManager<TUser> manager, TUser user)
        {
            return base.GenerateAsync(purpose, manager, user);
        }
        public override Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<TUser> manager, TUser user)
        {
            return base.CanGenerateTwoFactorTokenAsync(manager, user);
        }
    }
}