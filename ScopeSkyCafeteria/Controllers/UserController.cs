using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ScopeSkyCafeteria.Data;
using ScopeSkyCafeteria.Models.Domain;
using ScopeSkyCafeteria.Models.DTOs;
using ScopeSkyCafeteria.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace ScopeSkyCafeteria.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository userRepository;
        private readonly SSCafeteriaDbContext dbContext;
        private readonly ITokenRepository tokenRepository;
        private readonly IMapper mapper;
        private readonly UserManager<User> userManager;

        public UserController(IUserRepository userRepository, SSCafeteriaDbContext dbContext, ITokenRepository tokenRepository, IMapper mapper , UserManager<User> userManager)
        {
            this.userRepository = userRepository;
            this.dbContext = dbContext;
            this.tokenRepository = tokenRepository;
            this.mapper = mapper;
            this.userManager = userManager;
        }

        [HttpPost]
        [Route("User")]
      [Authorize(Roles = Roles.SuperAdmin + "," + Roles.Admin)]
        public async Task<IActionResult> CreateUser([FromBody] AddUserDTO addUserDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var existingEmail = await userManager.FindByEmailAsync(
                addUserDTO.Email);

            var existingUserName = await userManager.FindByNameAsync(
                addUserDTO.UserName);

            if (existingEmail != null)
            {
                return BadRequest("Email already exists.");
            }

            if (existingUserName != null)
            {
                return BadRequest("Username already exists.");
            }

            var identityUser = new User
            {
                FirstName = addUserDTO.FirstName,
                LastName = addUserDTO.LastName,
                UserName = addUserDTO.UserName,
                Email = addUserDTO.Email
            };

            var result = await userManager.CreateAsync(identityUser, addUserDTO.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }


            var role = string.IsNullOrEmpty(addUserDTO.Role) ? Roles.User : addUserDTO.Role;

            await userManager.AddToRoleAsync(identityUser, role);

            return Ok("User created successfully");
        }

        [HttpPost]
        [Route("Login")]

        public async Task<IActionResult> Login([FromBody] LoginRequestDTO loginRequestDTO)
        {
            var user = await userManager.FindByNameAsync(loginRequestDTO.UserName);

            if (user == null)
                return BadRequest("User not found");

            var checkPasswordResult = await userManager.CheckPasswordAsync(user, loginRequestDTO.Password);

            if (!checkPasswordResult)
                return BadRequest("Password is incorrect");

            var token = await tokenRepository.CreatJWTToken(user);

            var roles = await userManager.GetRolesAsync(user);

            return Ok(new LoginResponseDTO
            {
                JwtToken = token,
                UserName = user.UserName,
                Email = user.Email,
                Roles = roles.ToList()
            });
        }

        [HttpDelete]
        [Route("{Id:Guid}")]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> DeleteUser([FromRoute] Guid Id)
        {
            var deleteUser = await userManager.FindByIdAsync(Id.ToString());

            if (deleteUser == null)
            {
                return NotFound("The ID Is Incorrect");
            }

            var result = await userManager.DeleteAsync(deleteUser);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new
            {
                message = "The user has been deleted successfully",
                UserName = deleteUser.UserName
            });
        }

        [HttpGet]
        [Authorize(Roles = Roles.SuperAdmin + "," + Roles.Admin)]

        public async Task<IActionResult> GetAllUsers()
        {
            var users = await userRepository.GetAllUserAsync();
            var usersDTO = mapper.Map<List<UserDTO>>(users);
            return Ok(usersDTO);
        }
    }
}
