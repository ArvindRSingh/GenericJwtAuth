using GenericJwtAuth.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureTableIdentity.Web.StartupServices
{
    public static class ConstantsInitializer
    {
        public static void Initialize(Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            Constants.EmailSender = configuration.GetSection("MailSettings").GetSection("Sender").Value;
        }
    }
}
