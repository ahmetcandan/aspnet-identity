using System.ComponentModel.DataAnnotations;

namespace IdentityApi.Model
{
    public class RemoveUserModel
    {
        [Required(ErrorMessage = "User Name is required")]
        public string Username { get; set; }
    }
}
