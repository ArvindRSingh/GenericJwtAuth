using System.ComponentModel.DataAnnotations;

namespace GenericJwtAuth.DTO
{
    public class LoginDto
    {
        [Required]
        public string UserName { get; set; }

        public string NormalizedUserName
        {
            get
            {
                // below logic should match to that in AzureTableUser.NormalizedUserName setter.
                return this.UserName.ToLowerInvariant();
            }
        }
        [Required]
        public string Password { get; set; }
    }
}
