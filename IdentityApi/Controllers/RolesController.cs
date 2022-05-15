using IdentityApi.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
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
        public async Task<ResponseModel> Post([FromBody] CreateRoleModel model)
        {
            try
            {

                var roleExists = await roleManager.FindByNameAsync(model.Name);
                if (roleExists != null)
                    return ResponseModel.Fail("Role already exists!", StatusCodes.Status500InternalServerError);

                IdentityRole role = new IdentityRole()
                {
                    Name = model.Name,
                    NormalizedName = string.IsNullOrEmpty(model.NormalizedName)
                    ? model.Name.Replace(" ", "").ToUpper()
                    : model.NormalizedName
                };
                var result = await roleManager.CreateAsync(role);

                if (!result.Succeeded)
                    return ResponseModel.Fail("Role creation failed! Please check role details and try again.", StatusCodes.Status500InternalServerError);

                return ResponseModel.Success();
            }
            catch
            {
                return ResponseModel.Fail("An unexpected error has occurred!");
            }
        }

        [HttpPut]
        public async Task<ResponseModel> Put([FromBody] RoleModel model)
        {
            try
            {
                var role = await roleManager.FindByIdAsync(model.Id);
                if (role == null)
                    return ResponseModel.Fail("Role not found!", StatusCodes.Status500InternalServerError);

                role.Name = model.Name;
                role.NormalizedName = string.IsNullOrEmpty(model.NormalizedName)
                    ? model.Name.Replace(" ", "").ToUpper()
                    : model.NormalizedName;
                var result = await roleManager.UpdateAsync(role);

                if (!result.Succeeded)
                    return ResponseModel.Fail("Role update failed! Please check role details and try again.", StatusCodes.Status500InternalServerError);

                return ResponseModel.Success("Role updated successfully!");
            }
            catch
            {
                return ResponseModel.Fail("An unexpected error has occurred!");
            }
        }

        [HttpPut]
        [Route("editclaims")]
        public async Task<ResponseModel> EditClaims([FromBody] RoleClaimsModel model)
        {
            try
            {

                var role = await roleManager.FindByIdAsync(model.Id);
                if (role == null)
                    return ResponseModel.Fail("Role not found!", StatusCodes.Status500InternalServerError);

                var currentClaims = await roleManager.GetClaimsAsync(role);

                // delete roles
                foreach (var claim in currentClaims.Where(c => !model.Claims.Any(r => r.Type.Equals(c.Type))))
                    await roleManager.RemoveClaimAsync(role, claim);

                //add roles
                foreach (var claim in model.Claims.Where(r => !currentClaims.Any(c => c.Type.Equals(r.Type))))
                    await roleManager.AddClaimAsync(role, claim);

                return ResponseModel.Success("Role claims updated successfully!");
            }
            catch
            {
                return ResponseModel.Fail("An unexpected error has occurred!");
            }
        }

        [HttpDelete]
        public async Task<ResponseModel> Delete([FromBody] RoleModel model)
        {
            try
            {
                var role = await roleManager.FindByIdAsync(model.Id);
                if (role == null)
                    return ResponseModel.Fail("Role not found!", StatusCodes.Status500InternalServerError);

                var result = await roleManager.DeleteAsync(role);

                if (!result.Succeeded)
                    return ResponseModel.Fail("Role delete failed! Please check role details and try again.", StatusCodes.Status500InternalServerError);

                return ResponseModel.Success("Role deleted successfully!");
            }
            catch
            {
                return ResponseModel.Fail("An unexpected error has occurred!");
            }
        }

        [HttpGet]
        public ResponseModel<List<IdentityRole>> Get()
        {
            try
            {
                return ResponseModel<List<IdentityRole>>.Success(roleManager.Roles.ToList());
            }
            catch
            {
                return ResponseModel<List<IdentityRole>>.Fail("Failed to fetch role list.");
            }
        }
    }
}
