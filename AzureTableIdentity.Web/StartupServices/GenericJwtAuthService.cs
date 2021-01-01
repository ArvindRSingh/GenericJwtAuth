using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

namespace GenericJwtAuth.StartupServices
{
    public static class GenericJwtAuthService
    {
        public static void AddGenericJwtAuthService(this IServiceCollection services)
        {
            //Add authentication before adding MVC
            services.AddAuthentication(sharedOptions =>
            {
                sharedOptions.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(cfg =>
            {
                var keyByteArray = Encoding.ASCII.GetBytes(JwtTokenConfigurations.IssuerSigningKey);
                var signingKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(keyByteArray);

                cfg.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    ValidateAudience = true,
                    ValidAudience = JwtTokenConfigurations.Audience,
                    ValidateIssuer = true,
                    ValidIssuer = JwtTokenConfigurations.Issuer,
                    ClockSkew = TimeSpan.FromMinutes(0),
                    CryptoProviderFactory = CryptoProviderFactory.Default
                };
            });

            services.AddAuthorization(auth =>
            {
                var defaultPolicy = new AuthorizationPolicyBuilder(new string[] { JwtBearerDefaults.AuthenticationScheme })
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme‌​)
                    .RequireAuthenticatedUser()
                    .Build();
                auth.AddPolicy("Bearer", defaultPolicy);
                auth.DefaultPolicy = defaultPolicy;
            });

        }
    }
}
