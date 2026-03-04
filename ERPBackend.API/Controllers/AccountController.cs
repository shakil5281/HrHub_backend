using System.Collections.Generic;
using System.Threading.Tasks;
using ERPBackend.Core.DTOs;
using ERPBackend.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ERPBackend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        #region Branch Management
        [HttpGet("branches")]
        public async Task<ActionResult<List<BranchDto>>> GetBranches()
        {
            return Ok(await _accountService.GetBranchesAsync());
        }

        [HttpPost("branches")]
        public async Task<ActionResult<BranchDto>> CreateBranch(BranchDto branch)
        {
            return Ok(await _accountService.AddBranchAsync(branch));
        }

        [HttpPut("branches")]
        public async Task<IActionResult> UpdateBranch(BranchDto branch)
        {
            var result = await _accountService.UpdateBranchAsync(branch);
            if (!result) return NotFound();
            return Ok();
        }

        [HttpDelete("branches/{id}")]
        public async Task<IActionResult> DeleteBranch(int id)
        {
            var result = await _accountService.DeleteBranchAsync(id);
            if (!result) return NotFound();
            return Ok();
        }
        #endregion

        #region Transactions
        [HttpGet("transactions")]
        public async Task<ActionResult<List<AccountTransactionDto>>> GetTransactions([FromQuery] string? type, [FromQuery] string? fundSource)
        {
            return Ok(await _accountService.GetTransactionsAsync(type, fundSource));
        }

        [HttpPost("transactions")]
        public async Task<ActionResult<AccountTransactionDto>> CreateTransaction(AccountTransactionDto transaction)
        {
            try
            {
                return Ok(await _accountService.CreateTransactionAsync(transaction));
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Advance Payments
        [HttpGet("advances")]
        public async Task<ActionResult<List<AdvancePaymentDto>>> GetAdvances()
        {
            return Ok(await _accountService.GetAdvancesAsync());
        }

        [HttpPost("advances")]
        public async Task<ActionResult<AdvancePaymentDto>> CreateAdvance(AdvancePaymentDto advance)
        {
            return Ok(await _accountService.CreateAdvanceAsync(advance));
        }
        #endregion

        #region Dashboard & Reports
        [HttpGet("summary")]
        public async Task<ActionResult<AccountDashboardSummaryDto>> GetSummary()
        {
            return Ok(await _accountService.GetDashboardSummaryAsync());
        }

        [HttpGet("reports/ledger")]
        public async Task<ActionResult<List<GeneralReportDto>>> GetLedgerReport([FromQuery] int? branchId, [FromQuery] string? fundSource)
        {
            return Ok(await _accountService.GetLedgerReportAsync(branchId, fundSource));
        }
        #endregion
    }
}
