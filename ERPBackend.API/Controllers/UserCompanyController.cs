using ERPBackend.Core.Constants;
using ERPBackend.Core.DTOs;
using ERPBackend.Core.Models;
using ERPBackend.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERPBackend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin)]
    public class UserCompanyController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserCompanyController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("assign")]
        public async Task<IActionResult> AssignCompanies(AssignCompanyDto model)
        {
            var user = await _context.Users
                .Include(u => u.AssignedCompanies)
                .FirstOrDefaultAsync(u => u.Id == model.UserId);

            if (user == null)
            {
                return NotFound("User not found");
            }

            var companies = await _context.Companies
                .Where(c => model.CompanyIds.Contains(c.Id))
                .ToListAsync();

            if (companies.Count != model.CompanyIds.Count)
            {
                return BadRequest("One or more company IDs are invalid");
            }

            // Clear existing and add new or just add new? 
            // User said "assign multiple company assign a single user"
            // Usually we replace the list or append. Let's replace for clarity.
            user.AssignedCompanies.Clear();
            foreach (var company in companies)
            {
                user.AssignedCompanies.Add(company);
            }

            await _context.SaveChangesAsync();

            return Ok("Companies assigned successfully");
        }

        [HttpGet("user-companies/{userId}")]
        public async Task<ActionResult<IEnumerable<CompanyDto>>> GetUserAssignedCompanies(string userId)
        {
            var user = await _context.Users
                .Include(u => u.AssignedCompanies)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound("User not found");
            }

            return Ok(user.AssignedCompanies.Select(c => new CompanyDto
            {
                Id = c.Id,
                CompanyNameEn = c.CompanyNameEn,
                CompanyNameBn = c.CompanyNameBn,
                AddressEn = c.AddressEn,
                AddressBn = c.AddressBn,
                PhoneNumber = c.PhoneNumber,
                RegistrationNo = c.RegistrationNo,
                Industry = c.Industry,
                Email = c.Email,
                Status = c.Status,
                Founded = c.Founded,
                LogoPath = c.LogoPath,
                AuthorizeSignaturePath = c.AuthorizeSignaturePath
            }));
        }
    }
}
