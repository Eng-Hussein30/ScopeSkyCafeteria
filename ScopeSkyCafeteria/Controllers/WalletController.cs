using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScopeSkyCafeteria.Models.Domain;
using ScopeSkyCafeteria.Models.DTOs;
using ScopeSkyCafeteria.Repositories;
using System.Security.Claims;

namespace ScopeSkyCafeteria.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WalletController : ControllerBase
    {
        private readonly IWalletRepository walletRepository;
        private readonly IMapper mapper;

        public WalletController(
            IWalletRepository walletRepository,
            IMapper mapper)
        {
            this.walletRepository = walletRepository;
            this.mapper = mapper;
        }

        // =====================================================
        // User
        // Get My Wallet
        // =====================================================

        [HttpGet("my")]
        [Authorize(Roles = Roles.User)]
        public async Task<IActionResult> GetMyWallet()
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user identity.");
            }

            var wallet =
                await walletRepository.GetWalletByUserIdAsync(userId);

            if (wallet == null)
            {
                return NotFound("Wallet not found.");
            }

            return Ok(mapper.Map<WalletDTO>(wallet));
        }

        // =====================================================
        // Admin + SuperAdmin
        // Get All Wallets
        // =====================================================

        [HttpGet]
        [Authorize(Roles = Roles.Admin + "," + Roles.SuperAdmin)]
        public async Task<IActionResult> GetAllWallets()
        {
            var wallets =
                await walletRepository.GetAllWalletsAsync();

            var walletsDTO =
                mapper.Map<List<WalletDTO>>(wallets);

            return Ok(walletsDTO);
        }

        // =====================================================
        // Admin + SuperAdmin
        // Get Wallet By User Id
        // =====================================================

        [HttpGet("user/{userId:guid}")]
        [Authorize(Roles = Roles.Admin + "," + Roles.SuperAdmin)]
        public async Task<IActionResult> GetWalletByUserId(Guid userId)
        {
            var wallet =
                await walletRepository.GetWalletByUserIdAsync(userId);

            if (wallet == null)
            {
                return NotFound("Wallet not found.");
            }

            return Ok(mapper.Map<WalletDTO>(wallet));
        }

        // =====================================================
        // Admin + SuperAdmin
        // Deposit
        // إضافة مبلغ حقيقي للمحفظة
        // =====================================================

        [HttpPost("user/{userId:guid}/deposit")]
        [Authorize(Roles = Roles.Admin + "," + Roles.SuperAdmin)]
        public async Task<IActionResult> Deposit(
            Guid userId,
            [FromBody] WalletActionDTO actionDTO)
        {
            if (actionDTO.Amount <= 0)
            {
                return BadRequest("Amount must be greater than zero.");
            }

            var performedByUserId = GetCurrentUserId();

            if (performedByUserId == null)
            {
                return Unauthorized();
            }

            try
            {
                var wallet = await walletRepository.DepositAsync(
                    userId,
                    actionDTO.Amount,
                    performedByUserId.Value,
                    actionDTO.Note);

                if (wallet == null)
                {
                    return NotFound("Wallet not found.");
                }

                return Ok(new
                {
                    message = "Amount deposited successfully.",
                    wallet = mapper.Map<WalletDTO>(wallet)
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // =====================================================
        // Admin + SuperAdmin
        // Adjust Balance
        // =====================================================

        [HttpPut("user/{userId:guid}/balance")]
        [Authorize(Roles = Roles.Admin + "," + Roles.SuperAdmin)]
        public async Task<IActionResult> AdjustBalance(
            Guid userId,
            [FromBody] WalletActionDTO actionDTO)
        {
            var performedByUserId = GetCurrentUserId();

            if (performedByUserId == null)
            {
                return Unauthorized();
            }

            try
            {
                var wallet = await walletRepository.AdjustBalanceAsync(
                    userId,
                    actionDTO.Amount,
                    performedByUserId.Value,
                    actionDTO.Note);

                if (wallet == null)
                {
                    return NotFound("Wallet not found.");
                }

                return Ok(new
                {
                    message = "Wallet balance updated successfully.",
                    wallet = mapper.Map<WalletDTO>(wallet)
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // =====================================================
        // Admin + SuperAdmin
        // Adjust Debt
        // =====================================================

        [HttpPut("user/{userId:guid}/debt")]
        [Authorize(Roles = Roles.Admin + "," + Roles.SuperAdmin)]
        public async Task<IActionResult> AdjustDebt(
            Guid userId,
            [FromBody] WalletActionDTO actionDTO)
        {
            var performedByUserId = GetCurrentUserId();

            if (performedByUserId == null)
            {
                return Unauthorized();
            }

            try
            {
                var wallet = await walletRepository.AdjustDebtAsync(
                    userId,
                    actionDTO.Amount,
                    performedByUserId.Value,
                    actionDTO.Note);

                if (wallet == null)
                {
                    return NotFound("Wallet not found.");
                }

                return Ok(new
                {
                    message = "User debt updated successfully.",
                    wallet = mapper.Map<WalletDTO>(wallet)
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // =====================================================
        // Admin + SuperAdmin
        // Pay Debt
        // =====================================================

        [HttpPost("user/{userId:guid}/pay-debt")]
        [Authorize(Roles = Roles.Admin + "," + Roles.SuperAdmin)]
        public async Task<IActionResult> PayDebt(
           Guid userId,
           [FromBody] WalletActionDTO actionDTO)
        {
            var performedByUserId = GetCurrentUserId();

            if (performedByUserId == null)
            {
                return Unauthorized();
            }

            try
            {
                var wallet = await walletRepository.PayDebtAsync(
                    userId,
                    actionDTO.Amount,
                    performedByUserId.Value,
                    actionDTO.Note);

                if (wallet == null)
                {
                    return NotFound("Wallet not found.");
                }

                return Ok(new
                {
                    message = "Debt payment completed successfully.",
                    wallet = mapper.Map<WalletDTO>(wallet)
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // =====================================================
        // Helper
        // =====================================================

        private Guid? GetCurrentUserId()
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (Guid.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }

            return null;
        }

        [HttpGet("user/{userId:guid}/transactions")]
        [Authorize(Roles = Roles.Admin + "," + Roles.SuperAdmin)]
        public async Task<IActionResult> GetUserTransactions(Guid userId)
        {
            var transactions =
                await walletRepository.GetTransactionsByUserIdAsync(userId);

            var transactionsDTO =
                mapper.Map<List<WalletTransactionDTO>>(transactions);

            return Ok(transactionsDTO);
        }
    }
}


