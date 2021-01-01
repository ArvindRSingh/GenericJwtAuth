using Microsoft.Extensions.Configuration;
using System;

namespace GenericJwtAuth.StartupServices
{
    public static class JwtTokenConfigurations
    {
        public static bool ValidateIssuerSigningKey { get; private set; }
        public static string IssuerSigningKey { get; private set; }
        public static bool ValidateIssuer { get; private set; }
        public static string Issuer { get; private set; }
        public static bool ValidateAudience { get; private set; }
        public static string Audience { get; private set; }
        public static DateTime ExpiresInMinutes { get; private set; }
        public static DateTime NotBefore { get; internal set; }

        public static void Load(IConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            try
            {
                var config = configuration.GetSection("Jwt");

                ValidateIssuerSigningKey = config.GetValue<bool>("ValidateIssuerSigningKey", true);
                IssuerSigningKey = config.GetValue<string>("IssuerSigningKey");
                ValidateIssuer = config.GetValue<bool>("ValidateIssuer", true);
                Issuer = config.GetValue<string>("Issuer");
                ValidateAudience = config.GetValue<bool>("ValidateAudience", true);
                Audience = config.GetValue<string>("Audience");
                ExpiresInMinutes = DateTime.Now.AddMinutes(config.GetValue<double>("ExpiresInMinutes", defaultValue: 60));
                NotBefore = DateTime.Now.AddMinutes(config.GetValue<double>("NotBefore", defaultValue: 0));

            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
