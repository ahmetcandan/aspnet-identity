using System.ComponentModel.DataAnnotations;

namespace IdentityApi.Model
{
    public class CreateRoleModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        public string NormalizedName { get; set; }
    }
}
