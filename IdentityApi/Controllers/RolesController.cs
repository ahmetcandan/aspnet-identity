using IdentityApi.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = "admin")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration _configuration;

        public RolesController(RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            this.roleManager = roleManager;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreateRoleModel model)
        {
            var roleExists = await roleManager.FindByNameAsync(model.Name);
            if (roleExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Role already exists!" });

            IdentityRole role = new IdentityRole()
            {
                Name = model.Name,
                NormalizedName = string.IsNullOrEmpty(model.NormalizedName)
                ? model.Name.Replace(" ", "").ToUpper()
                : model.NormalizedName
            };
            var result = await roleManager.CreateAsync(role);

            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Role creation failed! Please check role details and try again." });

            return Ok(role);
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] RoleModel model)
        {
            var role = await roleManager.FindByIdAsync(model.Id);
            if (role == null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Role not found!" });

            role.Name = model.Name;
            role.NormalizedName = string.IsNullOrEmpty(model.NormalizedName)
                ? model.Name.Replace(" ", "").ToUpper()
                : model.NormalizedName;
            var result = await roleManager.UpdateAsync(role);

            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Role update failed! Please check role details and try again." });

            return Ok(new Response { Status = "Success", Message = "Role updated successfully!" });
        }

        [HttpPut]
        [Route("editclaims")]
        public async Task<IActionResult> EditClaims([FromBody] RoleClaimsModel model)
        {
            var role = await roleManager.FindByIdAsync(model.Id);
            if (role == null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Role not found!" });

            var currentClaims = await roleManager.GetClaimsAsync(role);

            // delete roles
            foreach (var claim in currentClaims.Where(c => !model.Claims.Any(r => r.Type.Equals(c.Type))))
                await roleManager.RemoveClaimAsync(role, claim);

            //add roles
            foreach (var claim in model.Claims.Where(r => !currentClaims.Any(c => c.Type.Equals(r.Type))))
                await roleManager.AddClaimAsync(role, claim);

            return Ok(new Response { Status = "Success", Message = "Role claims updated successfully!" });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] RoleModel model)
        {
            var role = await roleManager.FindByIdAsync(model.Id);
            if (role == null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Role not found!" });

            var result = await roleManager.DeleteAsync(role);

            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Role delete failed! Please check role details and try again." });

            return Ok(new Response { Status = "Success", Message = "Role deleted successfully!" });
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(roleManager.Roles.ToList());
        }
    }
}
