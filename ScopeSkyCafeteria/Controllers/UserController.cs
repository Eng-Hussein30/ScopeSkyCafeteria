using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public async Task<IActionResult> CreateUser([FromBody] AddUserDTO addUserDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

          // ==========================================
          // Determine Role
          // ==========================================

var role = string.IsNullOrWhiteSpace(addUserDTO.Role)
    ? Roles.User
    : addUserDTO.Role.Trim();

            // ==========================================
            // Validate Role
            // ==========================================

            if (role != Roles.User &&
                role != Roles.Admin &&
                role != Roles.SuperAdmin)
            {
                return BadRequest(new
                {
                    message = "Invalid role."
                });
            }

            // ==========================================
            // Admin cannot create SuperAdmin
            // IMPORTANT:
            // This check MUST happen before CreateAsync
            // ==========================================

            if (User.IsInRole(Roles.Admin) &&
                role == Roles.SuperAdmin)
            {
                return Forbid();
            }

            // ==========================================
            // Validate Department
            // ==========================================

            string? departmentName = null;

            if (role == Roles.User)
            {
                if (string.IsNullOrWhiteSpace(addUserDTO.DepartmentName))
                {
                    return BadRequest(new
                    {
                        message = "Department name is required for users."
                    });
                }

                departmentName =
                    addUserDTO.DepartmentName.Trim();
            }

            // ==========================================
            // Check Email
            // ==========================================

            var existingEmail =
                await userManager.FindByEmailAsync(addUserDTO.Email);

            if (existingEmail != null)
            {
                return BadRequest("Email already exists.");
            }

            // ==========================================
            // Check Username
            // ==========================================

            var existingUserName =
                await userManager.FindByNameAsync(addUserDTO.UserName);

            if (existingUserName != null)
            {
                return BadRequest("Username already exists.");
            }

            // ==========================================
            // Check Phone Number
            // ==========================================

            var existingPhoneNumber =
                await userManager.Users
                    .FirstOrDefaultAsync(
                        u => u.PhoneNumber == addUserDTO.PhoneNumber);

            if (existingPhoneNumber != null)
            {
                return BadRequest("Phone number already exists.");
            }

            // ==========================================
            // Create Identity User
            // ==========================================

            var identityUser = new User
            {
                FirstName = addUserDTO.FirstName,
                LastName = addUserDTO.LastName,
                UserName = addUserDTO.UserName,
                PhoneNumber = addUserDTO.PhoneNumber,
                Email = addUserDTO.Email,
                DepartmentName = departmentName,
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

            // ==========================================
            // Assign Role
            // ==========================================

            var roleResult =
                await userManager.AddToRoleAsync(
                    identityUser,
                    role);

            if (!roleResult.Succeeded)
            {
                // Rollback created Identity user
                await userManager.DeleteAsync(identityUser);

                return BadRequest(roleResult.Errors);
            }

            // ==========================================
            // Create Wallet Automatically
            // ==========================================

            try
            {
                await walletRepository.CreateWalletAsync(
                    identityUser.Id);
            }
            catch
            {
                // Rollback user if wallet creation fails
                await userManager.DeleteAsync(identityUser);

                throw;
            }

            // ==========================================
            // Response
            // ==========================================

            return Ok(new
            {
                message = "User created successfully",
                userId = identityUser.Id,
                userName = identityUser.UserName,
                phoneNumber = identityUser.PhoneNumber
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // البحث عن المستخدم بواسطة رقم الهاتف
            var user = await userManager.Users
                .FirstOrDefaultAsync(
                    u => u.PhoneNumber == loginRequestDTO.PhoneNumber);

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
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
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
                UserName = deleteUser.UserName ?? string.Empty
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