using IdentityApi.Model;
using Microsoft.AspNetCore.Authorization;
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
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration _configuration;

        public TokenController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            _configuration = configuration;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ResponseModel<TokenModel>> Login([FromBody] LoginModel model)
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

                    return ResponseModel<TokenModel>.Success(new TokenModel
                    {
                        Token = new JwtSecurityTokenHandler().WriteToken(token),
                        Expiration = token.ValidTo,
                        CreatedDate = now,
                        Claims = (from c in claims select new ClaimModel { Type = c.Type, Value = c.Value }).ToList()
                    });
                }
            }
            catch (Exception ex)
            {
                return ResponseModel<TokenModel>.Fail("Error: " + ex.Message, 500);
            }
            return ResponseModel<TokenModel>.Fail("Wrong username or password!", 500);
        }
    }
}
