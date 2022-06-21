using Homework5.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using WebApplication2.Data;
using WebApplication2.Models;
using WebApplication2.Services;

namespace JwtToken.Controllers
{


    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly TokenOption tokenOption;
        private readonly IMQService _mQService;
        public AccountController(UserManager<IdentityUser> userManager, IOptions<TokenOption> options, IMQService mQService) =>
            (_userManager, tokenOption,_mQService) = (userManager, options.Value,mQService);

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto login)
        {
            List<Claim> claims = new List<Claim>();
            var user = await _userManager.FindByEmailAsync(login.Email);
            if (user == null) throw new Exception("");

            var result = await _userManager.CheckPasswordAsync(user, login.Password);
            if (result)
            {

                var roles = await _userManager.GetRolesAsync(user);
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                claims.Add(new Claim(ClaimTypes.Name, user.Email));
                claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

                var token = GetToken(claims);

                var handler = new JwtSecurityTokenHandler();
                string jwt = handler.WriteToken(token);

                return Ok(new
                {
                    token = jwt,
                    expiration = token.ValidTo
                });
            }

            return Unauthorized();
        }
        
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto request)
        {
            if (_userManager.Users.Any(u => u.Email == request.Email))
            {
                return BadRequest("User already exists.");
            };

            var user = new IdentityUser { Email=request.Email ,UserName=request.UserName};

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.Select(x=>x));
            }
            _mQService.CreateMQ(user.UserName);
            return Ok("User Created");
        }
        [HttpGet]
        public IActionResult GetUsers([FromQuery] PagingQueryParameter parameter)
        {
            var users = _userManager.Users.AsQueryable();
            if (!string.IsNullOrWhiteSpace(parameter.Keyword))
            {
                users = users.Where(x => x.UserName.Contains(parameter.Keyword));
            }
            var totalUser = users.Count();
            var totalPage = (int)Math.Ceiling((double)totalUser / parameter.Limit);
            bool hasPrevious = parameter.Page > 1;
            var hasNext = parameter.Page < totalPage;
            var metaData = new
            {
                totalUser,
                parameter.Limit,
                parameter.Page,
                totalPage,
                hasPrevious,
                hasNext
            };
            Response.Headers.Add("X-Pagination",JsonSerializer.Serialize(metaData));
            return Ok(users.Skip((parameter.Page - 1)*parameter.Limit).Take(parameter.Limit));
        }



        private JwtSecurityToken GetToken(List<Claim> claims)
        {

            var signinKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenOption.Key));

            var token = new JwtSecurityToken(

                 signingCredentials: new SigningCredentials(signinKey, SecurityAlgorithms.HmacSha256),
                 issuer: tokenOption.Issuer,
                 audience: tokenOption.Audience,
                 expires: DateTime.Now.AddMinutes(tokenOption.Expiration),
                 claims: claims
                );

            return token;

        }

    }
}
