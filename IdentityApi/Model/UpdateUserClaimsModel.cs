using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace IdentityApi.Model
{
    public class UpdateUserClaimsModel
    {
        [Required(ErrorMessage = "User Name is required")]
        public string Username { get; set; }

        public IList<Claim> Claims { get; set; }
    }
}
