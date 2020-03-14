using System.ComponentModel.DataAnnotations;

namespace GenericJwtAuth.Controllers
{
    public class UserModel
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }
}