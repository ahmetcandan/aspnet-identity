using IdentityApi.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace IdentityApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = "admin")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration _configuration;

        public UsersController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            _configuration = configuration;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            try
            {
                var now = DateTime.Now;
                var user = await userManager.FindByNameAsync(model.Username);
                if (user != null && await userManager.CheckPasswordAsync(user, model.Password))
                {
                    var userRoles = await userManager.GetRolesAsync(user);

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.UserName),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    };

                    claims.AddRange(await userManager.GetClaimsAsync(user));
                    foreach (var roleName in userRoles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, roleName));
                        var role = await roleManager.FindByNameAsync(roleName);
                        claims.AddRange(await roleManager.GetClaimsAsync(role));
                    }

                    claims.Add(new Claim("LoginTime", now.ToString("O"), "DateTime[O]"));

                    var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

                    var token = new JwtSecurityToken(
                        issuer: _configuration["JWT:ValidIssuer"],
                        audience: _configuration["JWT:ValidAudience"],
                        expires: now.AddHours(5),
                        claims: claims,
                        signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                        );

                    return Ok(new
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(token),
                        expiration = token.ValidTo,
                        createdDate = now,
                        claims = (from c in claims select new { c.Type, c.Value, c.ValueType }).ToList()
                    });
                }
            }
            catch (Exception ex)
            {
                return Ok("Hata: " + ex.Message);
            }
            return Unauthorized();
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserModel model)
        {
            var userExists = await userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User already exists!" });

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
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });

            return Ok(new Response { Status = "Success", Message = "User created successfully!" });
        }

        [HttpPut]
        [Route("editroles")]
        public async Task<IActionResult> EditRoles([FromBody] UpdateUserRolesModel model)
        {
            var user = await userManager.FindByNameAsync(model.Username);
            if (user == null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User not found!" });

            var currentRoles = await userManager.GetRolesAsync(user);

            // delete roles
            await userManager.RemoveFromRolesAsync(user, currentRoles.Where(c => !model.Roles.Any(r => r.Equals(c))));

            //add roles
            await userManager.AddToRolesAsync(user, model.Roles.Where(r => !currentRoles.Any(c => c.Equals(r))));

            return Ok(new Response { Status = "Success", Message = "User updated roles successfully!" });
        }

        [HttpPut]
        [Route("editcliams")]
        public async Task<IActionResult> EditCliams([FromBody] UpdateUserClaimsModel model)
        {
            var user = await userManager.FindByNameAsync(model.Username);
            if (user == null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User not found!" });

            var currentClaims = await userManager.GetClaimsAsync(user);

            // delete roles
            await userManager.RemoveClaimsAsync(user, currentClaims.Where(c => !model.Claims.Any(r => r.Type.Equals(c.Type))));

            //add roles
            await userManager.AddClaimsAsync(user, model.Claims.Where(r => !currentClaims.Any(c => c.Type.Equals(r.Type))));

            return Ok(new Response { Status = "Success", Message = "User updated claims successfully!" });
        }

        [HttpGet]
        [Route("getuser")]
        public async Task<IActionResult> GetUser(string username)
        {
            var user = await userManager.FindByNameAsync(username);
            if (user == null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User not found!" });

            var currentRoles = await userManager.GetRolesAsync(user);
            return Ok(new UserModel()
            {
                Username = user.UserName,
                Email = user.Email,
                Roles = currentRoles,
                Id = user.Id
            });
        }

        [HttpGet]
        [Route("getallusers")]
        public async Task<IActionResult> GetAllUsers()
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

            return Ok(result);
        }

        [HttpDelete]
        [Route("removeuser")]
        public async Task<IActionResult> RemoveUser([FromBody] RemoveUserModel model)
        {
            var user = await userManager.FindByNameAsync(model.Username);
            if (user == null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User not found!" });

            await userManager.DeleteAsync(user);

            return Ok(new Response { Status = "Success", Message = "User removed successfully!" });
        }
    }
}
