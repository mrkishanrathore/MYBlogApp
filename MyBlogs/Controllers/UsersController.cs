using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBlogs.Models;
using MyBlogs.Models.Contexts;
using MyBlogs.Models.Entities;
using MyBlogs.Models.ModelsDtos;
using MyBlogs.Models.ModelsDTOs;

namespace MyBlogs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(UserCredentialsContext context) : ControllerBase
    {
        private readonly UserCredentialsContext _context = context;

        [HttpGet]
        async public Task<ActionResult<List<User>>> GetUsers()
        {
            return Ok(await _context.UserCredentials.ToListAsync());
        }

        [Route("adduser")]
        [HttpPost]
        async public Task<ActionResult<User>> PostNewUser(UserDto user)
        {
            var existingUser = await _context.UserCredentials.FirstOrDefaultAsync(e => e.Email == user.Email);
            if (user != null && existingUser == null) 
            {
                var hashPassword = new PasswordHasher();
                User newUser = new() { Email=user.Email, Password= hashPassword.HashPassword("",user.Password),UserName = user.UserName, UserId = Guid.NewGuid()};
                await _context.UserCredentials.AddAsync(newUser);
                _context.SaveChanges();
                return Ok(newUser);
            }
            return BadRequest("User Is Already Present");
        }
        

        [Route("checkuser")]
        [HttpPost]
        public async Task<ActionResult<User>> PostCheckUser(CheckUserDto user)
        {
            PasswordHasher ph = new PasswordHasher();
            User? matcheduser = await _context.UserCredentials.FirstOrDefaultAsync(e => e.Email == user.Email);
            if (matcheduser == null ) 
            {
                return BadRequest("User Do not Exist");
            }else if(ph.VerifyHashedPassword("",matcheduser.Password,user.Password) == PasswordVerificationResult.Success)
            {
                return matcheduser;
            }else if(ph.VerifyHashedPassword("", matcheduser.Password, user.Password) == PasswordVerificationResult.Failed)
            {
                return BadRequest("Password Does Not Match");
            }
            return BadRequest();
        }
    }
}
