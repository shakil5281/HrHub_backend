using System.Collections.Generic;
using System.Threading.Tasks;
using ERPBackend.Core.DTOs;
using ERPBackend.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPBackend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
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

        [HttpGet("transactions/{id}")]
        public async Task<ActionResult<AccountTransactionDto>> GetTransaction(int id)
        {
            var transaction = await _accountService.GetTransactionByIdAsync(id);
            if (transaction == null) return NotFound();
            return Ok(transaction);
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

        [HttpGet("reports/export/excel")]
        public async Task<IActionResult> ExportExcel([FromQuery] string? type, [FromQuery] string? fundSource)
        {
            var content = await _accountService.ExportTransactionsToExcelAsync(type, fundSource);
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Transactions_{DateTime.Now:yyyyMMddHHmm}.xlsx");
        }

        [HttpGet("reports/export/pdf")]
        public async Task<IActionResult> ExportPdf([FromQuery] string? type, [FromQuery] string? fundSource)
        {
            var content = await _accountService.ExportTransactionsToPdfAsync(type, fundSource);
            return File(content, "application/pdf", $"Transactions_{DateTime.Now:yyyyMMddHHmm}.pdf");
        }

        [HttpGet("transactions/{id}/export/excel")]
        public async Task<IActionResult> ExportVoucherExcel(int id)
        {
            try
            {
                var content = await _accountService.ExportVoucherExcelAsync(id);
                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Voucher_{id}_{DateTime.Now:yyyyMMdd}.xlsx");
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("transactions/{id}/export/pdf")]
        public async Task<IActionResult> ExportVoucherPdf(int id)
        {
            try
            {
                var content = await _accountService.ExportVoucherPdfAsync(id);
                return File(content, "application/pdf", $"Voucher_{id}_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        #endregion
    }
}
