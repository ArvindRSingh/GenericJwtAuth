using Microsoft.AspNetCore.Identity;

namespace GenericJwtAuth.Tests
{
    public class LookupNormalizer : ILookupNormalizer
    {
        public string NormalizeEmail(string email)
        {
            return email.ToLower();
        }

        public string NormalizeName(string name)
        {
            return name.ToLower();
        }
    }
}