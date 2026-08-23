using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ScopeSkyCafeteria.Models.Domain;
using ScopeSkyCafeteria.Models.DTOs;
using ScopeSkyCafeteria.Repositories;

namespace ScopeSkyCafeteria.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository userRepository;
        private readonly ITokenRepository tokenRepository;
        private readonly IMapper mapper;
        private readonly UserManager<User> userManager;
        private readonly IWalletRepository walletRepository;

        public UserController(
            IUserRepository userRepository,
            ITokenRepository tokenRepository,
            IMapper mapper,
            UserManager<User> userManager,
            IWalletRepository walletRepository)
        {
            this.userRepository = userRepository;
            this.tokenRepository = tokenRepository;
            this.mapper = mapper;
            this.userManager = userManager;
            this.walletRepository = walletRepository;
        }

        // ==========================================
        // Create User
        // Admin + SuperAdmin
        // ==========================================

        [HttpPost]
        [Route("User")]
        [Authorize(Roles = Roles.SuperAdmin + "," + Roles.Admin)]
        public async Task<IActionResult> CreateUser(
            [FromBody] AddUserDTO addUserDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingEmail =
                await userManager.FindByEmailAsync(addUserDTO.Email);

            var existingUserName =
                await userManager.FindByNameAsync(addUserDTO.UserName);

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
                Email = addUserDTO.Email,
                EmailConfirmed = true
            };

            var result =
                await userManager.CreateAsync(
                    identityUser,
                    addUserDTO.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            var role = string.IsNullOrEmpty(addUserDTO.Role)
                ? Roles.User
                : addUserDTO.Role;

            var roleResult =
                await userManager.AddToRoleAsync(
                    identityUser,
                    role);

            if (!roleResult.Succeeded)
            {
                await userManager.DeleteAsync(identityUser);

                return BadRequest(roleResult.Errors);
            }

            // إنشاء المحفظة تلقائياً
            await walletRepository.CreateWalletAsync(identityUser.Id);

            return Ok(new
            {
                message = "User created successfully",
                userId = identityUser.Id,
                userName = identityUser.UserName
            });
        }

        // ==========================================
        // Login
        // ==========================================

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequestDTO loginRequestDTO)
        {
            var user =
                await userManager.FindByNameAsync(
                    loginRequestDTO.UserName);

            if (user == null ||
                !await userManager.CheckPasswordAsync(
                    user,
                    loginRequestDTO.Password))
            {
                return BadRequest("Invalid credentials");
            }

            var token =
                await tokenRepository.CreatJWTToken(user);

            var roles =
                await userManager.GetRolesAsync(user);

            return Ok(new LoginResponseDTO
            {
                JwtToken = token,
                UserName = user.UserName,
                Email = user.Email,
                Roles = roles.ToList()
            });
        }

        // ==========================================
        // Delete User
        // SuperAdmin only
        // ==========================================

        [HttpDelete]
        [Route("{Id:Guid}")]
        [Authorize(Roles = Roles.SuperAdmin)]
        public async Task<IActionResult> DeleteUser(
            [FromRoute] Guid Id)
        {
            var deleteUser =
                await userManager.FindByIdAsync(Id.ToString());

            if (deleteUser == null)
            {
                return NotFound("The ID Is Incorrect");
            }

            var result =
                await userManager.DeleteAsync(deleteUser);

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

        // ==========================================
        // Get All Users
        // Admin + SuperAdmin
        // ==========================================

        [HttpGet]
        [Authorize(Roles = Roles.SuperAdmin + "," + Roles.Admin)]
        public async Task<IActionResult> GetAllUsers()
        {
            var users =
                await userRepository.GetAllUserAsync();

            var usersDTO =
                mapper.Map<List<UserDTO>>(users);

            return Ok(usersDTO);
        }
    }
}