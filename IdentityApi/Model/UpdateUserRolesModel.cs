using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IdentityApi.Model
{
    public class UpdateUserRolesModel
    {
        [Required(ErrorMessage = "User Name is required")]
        public string Username { get; set; }

        public IList<string> Roles { get; set; }
    }
}
