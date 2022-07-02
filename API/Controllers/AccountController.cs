using API.Data;
using API.DTO;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext context;
        private readonly ITokenService tokenService;

        public AccountController(DataContext context, ITokenService tokenService)
        {
            this.context = context;
            this.tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterUserDto registerUserDTO)
        {

            if (await CheckIfUserNameExists(registerUserDTO.UserName))
                return BadRequest("Username already exists");

            using var hmac = new HMACSHA512();

            var user = new User()
            {
                UserName = registerUserDTO.UserName.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerUserDTO.Password)),
                PasswordSalt = hmac.Key
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            return new UserDto
            {
                UserName = user.UserName,
                Token=tokenService.CreateToken(user)
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {

            var user = await context.Users.SingleOrDefaultAsync(x => x.UserName.Equals(loginDto.UserName.ToLower()));

            if (user == null)
                return Unauthorized("Invalid Username");

            using var hmac = new HMACSHA512(user.PasswordSalt);

            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i])
                    return Unauthorized("Invalid Password");
            }

            return new UserDto
            {
                UserName = user.UserName,
                Token = tokenService.CreateToken(user)
            };
        }

        private async Task<bool> CheckIfUserNameExists(string username)
        {
            return await context.Users.AnyAsync(x => x.UserName.Equals(username.ToLower()));
        }
    }
}
