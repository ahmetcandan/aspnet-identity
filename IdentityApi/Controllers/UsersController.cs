using IdentityApi.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = "admin")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;

        public UsersController(UserManager<IdentityUser> userManager)
        {
            this.userManager = userManager;
        }

        [HttpPost]
        [Route("register")]
        public async Task<ResponseModel> Register([FromBody] RegisterUserModel model)
        {
            try
            {
                var userExists = await userManager.FindByNameAsync(model.Username);
                if (userExists != null)
                    return ResponseModel.Fail("User already exists!", StatusCodes.Status500InternalServerError);

                var user = new IdentityUser()
                {
                    Email = model.Email,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    UserName = model.Username
                };
                var result = await userManager.CreateAsync(user, model.Password);
                if (result.Succeeded && model.Roles != null && model.Roles.Count > 0)
                    await userManager.AddToRolesAsync(user, model.Roles);
                if (!result.Succeeded)
                    return ResponseModel.Fail("User creation failed! Please check user details and try again.", StatusCodes.Status500InternalServerError);

                return ResponseModel.Success("User created successfully!");
            }
            catch
            {
                return ResponseModel.Fail("An unexpected error has occurred!");
            }
        }

        [HttpPut]
        [Route("editroles")]
        public async Task<ResponseModel> EditRoles([FromBody] UpdateUserRolesModel model)
        {
            try
            {
                var user = await userManager.FindByNameAsync(model.Username);
                if (user == null)
                    return ResponseModel.Fail("User not found!", StatusCodes.Status500InternalServerError);

                var currentRoles = await userManager.GetRolesAsync(user);

                // delete roles
                await userManager.RemoveFromRolesAsync(user, currentRoles.Where(c => !model.Roles.Any(r => r.Equals(c))));

                //add roles
                await userManager.AddToRolesAsync(user, model.Roles.Where(r => !currentRoles.Any(c => c.Equals(r))));

                return ResponseModel.Success("User updated roles successfully!");
            }
            catch
            {
                return ResponseModel.Fail("An unexpected error has occurred!");
            }
        }

        [HttpPut]
        [Route("editcliams")]
        public async Task<ResponseModel> EditCliams([FromBody] UpdateUserClaimsModel model)
        {
            try
            {

                var user = await userManager.FindByNameAsync(model.Username);
                if (user == null)
                    return ResponseModel.Fail("User not found!", StatusCodes.Status500InternalServerError);

                var currentClaims = await userManager.GetClaimsAsync(user);

                // delete roles
                await userManager.RemoveClaimsAsync(user, currentClaims.Where(c => !model.Claims.Any(r => r.Type.Equals(c.Type))));

                //add roles
                await userManager.AddClaimsAsync(user, model.Claims.Where(r => !currentClaims.Any(c => c.Type.Equals(r.Type))));

                return ResponseModel.Success("User updated claims successfully!");
            }
            catch
            {
                return ResponseModel.Fail("An unexpected error has occurred!");
            }
        }

        [HttpGet]
        [Route("getuser")]
        public async Task<ResponseModel<UserModel>> GetUser(string username)
        {
            try
            {
                var user = await userManager.FindByNameAsync(username);
                if (user == null)
                    return ResponseModel<UserModel>.Fail("User not found!", StatusCodes.Status500InternalServerError);

                var currentRoles = await userManager.GetRolesAsync(user);
                return ResponseModel<UserModel>.Success(new UserModel()
                {
                    Username = user.UserName,
                    Email = user.Email,
                    Roles = currentRoles,
                    Id = user.Id
                });
            }
            catch
            {
                return ResponseModel<UserModel>.Fail("Failed to fetch role list!");
            }
        }

        [HttpGet]
        [Route("getallusers")]
        public async Task<ResponseModel<List<UserModel>>> GetAllUsers()
        {
            try
            {
                var users = userManager.Users.ToList();

                var result = new List<UserModel>();
                foreach (var user in users)
                    result.Add(new UserModel()
                    {
                        Username = user.UserName,
                        Email = user.Email,
                        Roles = await userManager.GetRolesAsync(user),
                        Id = user.Id
                    });

                return ResponseModel<List<UserModel>>.Success(result);
            }
            catch
            {
                return ResponseModel<List<UserModel>>.Fail("Failed to fetch role list!");
            }
        }

        [HttpDelete]
        [Route("removeuser")]
        public async Task<ResponseModel> RemoveUser([FromBody] RemoveUserModel model)
        {
            try
            {
                var user = await userManager.FindByNameAsync(model.Username);
                if (user == null)
                    return ResponseModel.Fail("User not found!", StatusCodes.Status500InternalServerError);

                await userManager.DeleteAsync(user);

                return ResponseModel.Success("User removed successfully!");
            }
            catch
            {
                return ResponseModel.Fail("An unexpected error has occurred!");
            }
        }
    }
}
