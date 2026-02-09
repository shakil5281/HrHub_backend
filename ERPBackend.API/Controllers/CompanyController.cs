using ERPBackend.Core.Constants;
using ERPBackend.Core.DTOs;
using ERPBackend.Core.Models;
using ERPBackend.Core.Enums;
using ERPBackend.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERPBackend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanyController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public CompanyController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CompanyDto>>> GetCompanies()
        {
            var user = await _context.Users
                .Include(u => u.AssignedCompanies)
                .FirstOrDefaultAsync(u => u.UserName == User.Identity!.Name);

            if (user == null) return Unauthorized();

            IQueryable<Company> query = _context.Companies;

            var roles = User.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role).Select(c => c.Value)
                .ToList();
            bool isAdmin = roles.Contains(UserRoles.SuperAdmin) || roles.Contains(UserRoles.Admin);

            if (!isAdmin)
            {
                var assignedIds = user.AssignedCompanies.Select(ac => ac.Id).ToList();
                query = query.Where(c => assignedIds.Contains(c.Id));
            }

            return await query
                .Select(c => new CompanyDto
                {
                    Id = c.Id,
                    Branch = c.Branch,
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
                })
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CompanyDto>> GetCompany(int id)
        {
            var roles = User.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role).Select(c => c.Value)
                .ToList();
            bool isAdmin = roles.Contains(UserRoles.SuperAdmin) || roles.Contains(UserRoles.Admin);

            var company = await _context.Companies.FindAsync(id);

            if (company == null)
            {
                return NotFound();
            }

            if (!isAdmin)
            {
                var user = await _context.Users
                    .Include(u => u.AssignedCompanies)
                    .FirstOrDefaultAsync(u => u.UserName == User.Identity!.Name);

                if (user == null || !user.AssignedCompanies.Any(ac => ac.Id == id))
                {
                    return Forbid();
                }
            }

            return new CompanyDto
            {
                Id = company.Id,
                Branch = company.Branch,
                CompanyNameEn = company.CompanyNameEn,
                CompanyNameBn = company.CompanyNameBn,
                AddressEn = company.AddressEn,
                AddressBn = company.AddressBn,
                PhoneNumber = company.PhoneNumber,
                RegistrationNo = company.RegistrationNo,
                Industry = company.Industry,
                Email = company.Email,
                Status = company.Status,
                Founded = company.Founded,
                LogoPath = company.LogoPath,
                AuthorizeSignaturePath = company.AuthorizeSignaturePath
            };
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin)]
        public async Task<ActionResult<CompanyDto>> CreateCompany([FromForm] CreateCompanyDto createCompanyDto)
        {
            if (await _context.Companies.AnyAsync(c => c.RegistrationNo == createCompanyDto.RegistrationNo))
            {
                return BadRequest("Registration number already exists.");
            }

            var company = new Company
            {
                Branch = createCompanyDto.Branch,
                CompanyNameEn = createCompanyDto.CompanyNameEn,
                CompanyNameBn = createCompanyDto.CompanyNameBn,
                AddressEn = createCompanyDto.AddressEn,
                AddressBn = createCompanyDto.AddressBn,
                PhoneNumber = createCompanyDto.PhoneNumber,
                RegistrationNo = createCompanyDto.RegistrationNo,
                Industry = createCompanyDto.Industry,
                Email = createCompanyDto.Email,
                Status = createCompanyDto.Status,
                Founded = createCompanyDto.Founded
            };

            if (company.Branch == BranchType.Primary)
            {
                var otherCompanies = await _context.Companies.Where(c => c.Branch == BranchType.Primary).ToListAsync();
                foreach (var other in otherCompanies)
                {
                    other.Branch = BranchType.Secondary;
                }
            }

            if (createCompanyDto.Logo != null)
            {
                company.LogoPath = await SaveFileAsync(createCompanyDto.Logo, "logos");
            }

            if (createCompanyDto.AuthorizeSignature != null)
            {
                company.AuthorizeSignaturePath = await SaveFileAsync(createCompanyDto.AuthorizeSignature, "signatures");
            }

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCompany), new { id = company.Id }, new CompanyDto
            {
                Id = company.Id,
                Branch = company.Branch,
                CompanyNameEn = company.CompanyNameEn,
                CompanyNameBn = company.CompanyNameBn,
                AddressEn = company.AddressEn,
                AddressBn = company.AddressBn,
                PhoneNumber = company.PhoneNumber,
                RegistrationNo = company.RegistrationNo,
                Industry = company.Industry,
                Email = company.Email,
                Status = company.Status,
                Founded = company.Founded,
                LogoPath = company.LogoPath,
                AuthorizeSignaturePath = company.AuthorizeSignaturePath
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin)]
        public async Task<IActionResult> UpdateCompany(int id, [FromForm] CreateCompanyDto updateCompanyDto)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null)
            {
                return NotFound();
            }

            if (await _context.Companies.AnyAsync(c =>
                    c.RegistrationNo == updateCompanyDto.RegistrationNo && c.Id != id))
            {
                return BadRequest("Registration number already exists.");
            }

            company.Branch = updateCompanyDto.Branch;
            company.CompanyNameEn = updateCompanyDto.CompanyNameEn;
            company.CompanyNameBn = updateCompanyDto.CompanyNameBn;
            company.AddressEn = updateCompanyDto.AddressEn;
            company.AddressBn = updateCompanyDto.AddressBn;
            company.PhoneNumber = updateCompanyDto.PhoneNumber;
            company.RegistrationNo = updateCompanyDto.RegistrationNo;
            company.Industry = updateCompanyDto.Industry;
            company.Email = updateCompanyDto.Email;
            company.Status = updateCompanyDto.Status;
            company.Founded = updateCompanyDto.Founded;
            company.UpdatedAt = DateTime.UtcNow;

            if (company.Branch == BranchType.Primary)
            {
                var otherCompanies = await _context.Companies
                    .Where(c => c.Branch == BranchType.Primary && c.Id != id)
                    .ToListAsync();
                foreach (var other in otherCompanies)
                {
                    other.Branch = BranchType.Secondary;
                }
            }

            if (updateCompanyDto.Logo != null)
            {
                if (!string.IsNullOrEmpty(company.LogoPath))
                {
                    DeleteFile(company.LogoPath);
                }

                company.LogoPath = await SaveFileAsync(updateCompanyDto.Logo, "logos");
            }

            if (updateCompanyDto.AuthorizeSignature != null)
            {
                if (!string.IsNullOrEmpty(company.AuthorizeSignaturePath))
                {
                    DeleteFile(company.AuthorizeSignaturePath);
                }

                company.AuthorizeSignaturePath = await SaveFileAsync(updateCompanyDto.AuthorizeSignature, "signatures");
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<string> SaveFileAsync(IFormFile file, string subfolder)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "companies", subfolder);
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/uploads/companies/{subfolder}/{fileName}";
        }

        private void DeleteFile(string relativePath)
        {
            var filePath = Path.Combine(_environment.WebRootPath, relativePath.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin)]
        public async Task<IActionResult> DeleteCompany(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null)
            {
                return NotFound();
            }

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
