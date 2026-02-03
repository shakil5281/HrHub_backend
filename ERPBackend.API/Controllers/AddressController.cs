using ERPBackend.Core.Constants;
using ERPBackend.Core.DTOs;
using ERPBackend.Core.Models;
using ERPBackend.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;


namespace ERPBackend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AddressController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AddressController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- Countries ---
        [HttpGet("countries")]
        public async Task<ActionResult<IEnumerable<CountryDto>>> GetCountries()
        {
            return await _context.Countries
                .Select(c => new CountryDto
                {
                    Id = c.Id,
                    NameEn = c.NameEn,
                    NameBn = c.NameBn
                })
                .ToListAsync();
        }

        [HttpPost("countries")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," +
                           UserRoles.HrOfficer + "," + UserRoles.ItOfficer)]
        public async Task<ActionResult<CountryDto>> CreateCountry(CreateCountryDto dto)
        {
            var country = new Country { NameEn = dto.NameEn, NameBn = dto.NameBn };
            _context.Countries.Add(country);
            await _context.SaveChangesAsync();
            return Ok(new CountryDto { Id = country.Id, NameEn = country.NameEn, NameBn = country.NameBn });
        }

        [HttpPut("countries/{id}")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," +
                           UserRoles.HrOfficer + "," + UserRoles.ItOfficer)]
        public async Task<IActionResult> UpdateCountry(int id, CreateCountryDto dto)
        {
            var country = await _context.Countries.FindAsync(id);
            if (country == null) return NotFound();
            country.NameEn = dto.NameEn;
            country.NameBn = dto.NameBn;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("countries/{id}")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," +
                           UserRoles.HrOfficer + "," + UserRoles.ItOfficer)]
        public async Task<IActionResult> DeleteCountry(int id)
        {
            var country = await _context.Countries.FindAsync(id);
            if (country == null) return NotFound();
            _context.Countries.Remove(country);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // --- Divisions ---
        [HttpGet("divisions")]
        public async Task<ActionResult<IEnumerable<DivisionDto>>> GetDivisions(int? countryId)
        {
            var query = _context.Divisions.AsQueryable();
            if (countryId.HasValue)
            {
                query = query.Where(d => d.CountryId == countryId);
            }

            return await query
                .Select(d => new DivisionDto
                {
                    Id = d.Id,
                    NameEn = d.NameEn,
                    NameBn = d.NameBn,
                    CountryId = d.CountryId,
                    CountryName = d.Country != null ? d.Country.NameEn : null
                })
                .ToListAsync();
        }

        [HttpPost("divisions")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," +
                           UserRoles.HrOfficer + "," + UserRoles.ItOfficer)]
        public async Task<ActionResult<DivisionDto>> CreateDivision(CreateDivisionDto dto)
        {
            var division = new Division { NameEn = dto.NameEn, NameBn = dto.NameBn, CountryId = dto.CountryId };
            _context.Divisions.Add(division);
            await _context.SaveChangesAsync();
            return Ok(new DivisionDto
            {
                Id = division.Id, NameEn = division.NameEn, NameBn = division.NameBn, CountryId = division.CountryId
            });
        }

        [HttpPut("divisions/{id}")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," +
                           UserRoles.HrOfficer + "," + UserRoles.ItOfficer)]
        public async Task<IActionResult> UpdateDivision(int id, CreateDivisionDto dto)
        {
            var division = await _context.Divisions.FindAsync(id);
            if (division == null) return NotFound();
            division.NameEn = dto.NameEn;
            division.NameBn = dto.NameBn;
            division.CountryId = dto.CountryId;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("divisions/{id}")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," +
                           UserRoles.HrOfficer + "," + UserRoles.ItOfficer)]
        public async Task<IActionResult> DeleteDivision(int id)
        {
            var division = await _context.Divisions.FindAsync(id);
            if (division == null) return NotFound();
            _context.Divisions.Remove(division);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // --- Districts ---
        [HttpGet("districts")]
        public async Task<ActionResult<IEnumerable<DistrictDto>>> GetDistricts(int? divisionId)
        {
            var query = _context.Districts.AsQueryable();
            if (divisionId.HasValue)
            {
                query = query.Where(d => d.DivisionId == divisionId);
            }

            return await query
                .Select(d => new DistrictDto
                {
                    Id = d.Id,
                    NameEn = d.NameEn,
                    NameBn = d.NameBn,
                    DivisionId = d.DivisionId,
                    DivisionName = d.Division != null ? d.Division.NameEn : null
                })
                .ToListAsync();
        }

        [HttpPost("districts")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," +
                           UserRoles.HrOfficer + "," + UserRoles.ItOfficer)]
        public async Task<ActionResult<DistrictDto>> CreateDistrict(CreateDistrictDto dto)
        {
            var district = new District { NameEn = dto.NameEn, NameBn = dto.NameBn, DivisionId = dto.DivisionId };
            _context.Districts.Add(district);
            await _context.SaveChangesAsync();
            return Ok(new DistrictDto
            {
                Id = district.Id, NameEn = district.NameEn, NameBn = district.NameBn, DivisionId = district.DivisionId
            });
        }

        [HttpPut("districts/{id}")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," +
                           UserRoles.HrOfficer + "," + UserRoles.ItOfficer)]
        public async Task<IActionResult> UpdateDistrict(int id, CreateDistrictDto dto)
        {
            var district = await _context.Districts.FindAsync(id);
            if (district == null) return NotFound();
            district.NameEn = dto.NameEn;
            district.NameBn = dto.NameBn;
            district.DivisionId = dto.DivisionId;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("districts/{id}")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," +
                           UserRoles.HrOfficer + "," + UserRoles.ItOfficer)]
        public async Task<IActionResult> DeleteDistrict(int id)
        {
            var district = await _context.Districts.FindAsync(id);
            if (district == null) return NotFound();
            _context.Districts.Remove(district);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // --- Thanas ---
        [HttpGet("thanas")]
        public async Task<ActionResult<IEnumerable<ThanaDto>>> GetThanas(int? districtId)
        {
            var query = _context.Thanas.AsQueryable();
            if (districtId.HasValue)
            {
                query = query.Where(t => t.DistrictId == districtId);
            }

            return await query
                .Select(t => new ThanaDto
                {
                    Id = t.Id,
                    NameEn = t.NameEn,
                    NameBn = t.NameBn,
                    DistrictId = t.DistrictId,
                    DistrictName = t.District != null ? t.District.NameEn : null
                })
                .ToListAsync();
        }

        [HttpPost("thanas")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," +
                           UserRoles.HrOfficer + "," + UserRoles.ItOfficer)]
        public async Task<ActionResult<ThanaDto>> CreateThana(CreateThanaDto dto)
        {
            var thana = new Thana { NameEn = dto.NameEn, NameBn = dto.NameBn, DistrictId = dto.DistrictId };
            _context.Thanas.Add(thana);
            await _context.SaveChangesAsync();
            return Ok(new ThanaDto
                { Id = thana.Id, NameEn = thana.NameEn, NameBn = thana.NameBn, DistrictId = thana.DistrictId });
        }

        [HttpPut("thanas/{id}")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," +
                           UserRoles.HrOfficer + "," + UserRoles.ItOfficer)]
        public async Task<IActionResult> UpdateThana(int id, CreateThanaDto dto)
        {
            var thana = await _context.Thanas.FindAsync(id);
            if (thana == null) return NotFound();
            thana.NameEn = dto.NameEn;
            thana.NameBn = dto.NameBn;
            thana.DistrictId = dto.DistrictId;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("thanas/{id}")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," +
                           UserRoles.HrOfficer + "," + UserRoles.ItOfficer)]
        public async Task<IActionResult> DeleteThana(int id)
        {
            var thana = await _context.Thanas.FindAsync(id);
            if (thana == null) return NotFound();
            _context.Thanas.Remove(thana);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // --- Post Offices ---
        [HttpGet("postoffices")]
        public async Task<ActionResult<IEnumerable<PostOfficeDto>>> GetPostOffices(int? districtId)
        {
            var query = _context.PostOffices.AsQueryable();
            if (districtId.HasValue)
            {
                query = query.Where(p => p.DistrictId == districtId);
            }

            return await query
                .Select(p => new PostOfficeDto
                {
                    Id = p.Id,
                    NameEn = p.NameEn,
                    NameBn = p.NameBn,
                    Code = p.Code,
                    DistrictId = p.DistrictId,
                    DistrictName = p.District != null ? p.District.NameEn : null
                })
                .ToListAsync();
        }

        [HttpPost("postoffices")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," +
                           UserRoles.HrOfficer + "," + UserRoles.ItOfficer)]
        public async Task<ActionResult<PostOfficeDto>> CreatePostOffice(CreatePostOfficeDto dto)
        {
            var po = new PostOffice
                { NameEn = dto.NameEn, NameBn = dto.NameBn, Code = dto.Code, DistrictId = dto.DistrictId };
            _context.PostOffices.Add(po);
            await _context.SaveChangesAsync();
            return Ok(new PostOfficeDto
                { Id = po.Id, NameEn = po.NameEn, NameBn = po.NameBn, Code = po.Code, DistrictId = po.DistrictId });
        }

        [HttpPut("postoffices/{id}")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," +
                           UserRoles.HrOfficer + "," + UserRoles.ItOfficer)]
        public async Task<IActionResult> UpdatePostOffice(int id, CreatePostOfficeDto dto)
        {
            var po = await _context.PostOffices.FindAsync(id);
            if (po == null) return NotFound();
            po.NameEn = dto.NameEn;
            po.NameBn = dto.NameBn;
            po.Code = dto.Code;
            po.DistrictId = dto.DistrictId;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("postoffices/{id}")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," +
                           UserRoles.HrOfficer + "," + UserRoles.ItOfficer)]
        public async Task<IActionResult> DeletePostOffice(int id)
        {
            var po = await _context.PostOffices.FindAsync(id);
            if (po == null) return NotFound();
            _context.PostOffices.Remove(po);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // --- Excel Import/Export ---
        [HttpGet("export-template")]
        [AllowAnonymous]
        public IActionResult ExportTemplate()
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Address Data");

            // Headers
            worksheet.Cells[1, 1].Value = "Country Name (EN)";
            worksheet.Cells[1, 2].Value = "Country Name (BN)";
            worksheet.Cells[1, 3].Value = "Division Name (EN)";
            worksheet.Cells[1, 4].Value = "Division Name (BN)";
            worksheet.Cells[1, 5].Value = "District Name (EN)";
            worksheet.Cells[1, 6].Value = "District Name (BN)";
            worksheet.Cells[1, 7].Value = "Thana Name (EN)";
            worksheet.Cells[1, 8].Value = "Thana Name (BN)";
            worksheet.Cells[1, 9].Value = "Post Office Name (EN)";
            worksheet.Cells[1, 10].Value = "Post Office Name (BN)";
            worksheet.Cells[1, 11].Value = "Post Code";

            // Style headers
            using (var range = worksheet.Cells[1, 1, 1, 11])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            }

            // Sample data
            worksheet.Cells[2, 1].Value = "Bangladesh";
            worksheet.Cells[2, 2].Value = "বাংলাদেশ";
            worksheet.Cells[2, 3].Value = "Dhaka";
            worksheet.Cells[2, 4].Value = "ঢাকা";
            worksheet.Cells[2, 5].Value = "Gazipur";
            worksheet.Cells[2, 6].Value = "গাজীপুর";
            worksheet.Cells[2, 7].Value = "Tongi";
            worksheet.Cells[2, 8].Value = "টঙ্গী";
            worksheet.Cells[2, 9].Value = "Tongi College Gate";
            worksheet.Cells[2, 10].Value = "টঙ্গী কলেজ গেট";
            worksheet.Cells[2, 11].Value = "1711";

            worksheet.Cells.AutoFitColumns();

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;


            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Address_Template.xlsx");
        }

        [HttpGet("export-demo")]
        [AllowAnonymous]
        public IActionResult ExportDemo()
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Address Data");

            // Headers
            worksheet.Cells[1, 1].Value = "Country Name (EN)";
            worksheet.Cells[1, 2].Value = "Country Name (BN)";
            worksheet.Cells[1, 3].Value = "Division Name (EN)";
            worksheet.Cells[1, 4].Value = "Division Name (BN)";
            worksheet.Cells[1, 5].Value = "District Name (EN)";
            worksheet.Cells[1, 6].Value = "District Name (BN)";
            worksheet.Cells[1, 7].Value = "Thana Name (EN)";
            worksheet.Cells[1, 8].Value = "Thana Name (BN)";
            worksheet.Cells[1, 9].Value = "Post Office Name (EN)";
            worksheet.Cells[1, 10].Value = "Post Office Name (BN)";
            worksheet.Cells[1, 11].Value = "Post Code";

            // Style headers
            using (var range = worksheet.Cells[1, 1, 1, 11])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            }

            // Demo data with multiple records
            var demoData =
                new List<(string, string, string, string, string, string, string, string, string, string, string)>
                {
                    ("Bangladesh", "বাংলাদেশ", "Dhaka", "ঢাকা", "Dhaka", "ঢাকা", "Dhanmondi", "ধানমন্ডি", "Dhanmondi",
                        "ধানমন্ডি", "1209"),
                    ("Bangladesh", "বাংলাদেশ", "Dhaka", "ঢাকা", "Dhaka", "ঢাকা", "Gulshan", "গুলশান", "Gulshan 1",
                        "গুলশান ১", "1212"),
                    ("Bangladesh", "বাংলাদেশ", "Dhaka", "ঢাকা", "Gazipur", "গাজীপুর", "Tongi", "টঙ্গী",
                        "Tongi College Gate", "টঙ্গী কলেজ গেট", "1711"),
                    ("Bangladesh", "বাংলাদেশ", "Dhaka", "ঢাকা", "Gazipur", "গাজীপুর", "Kaliakair", "কালিয়াকৈর",
                        "Kaliakair", "কালিয়াকৈর", "1750"),
                    ("Bangladesh", "বাংলাদেশ", "Chittagong", "চট্টগ্রাম", "Chittagong", "চট্টগ্রাম", "Agrabad",
                        "আগ্রাবাদ", "Agrabad", "আগ্রাবাদ", "4100"),
                    ("Bangladesh", "বাংলাদেশ", "Chittagong", "চট্টগ্রাম", "Chittagong", "চট্টগ্রাম", "Patenga",
                        "পতেঙ্গা", "Patenga", "পতেঙ্গা", "4202"),
                    ("Bangladesh", "বাংলাদেশ", "Chittagong", "চট্টগ্রাম", "Cox's Bazar", "কক্সবাজার", "Sadar", "সদর",
                        "Cox's Bazar Sadar", "কক্সবাজার সদর", "4700"),
                    ("Bangladesh", "বাংলাদেশ", "Rajshahi", "রাজশাহী", "Rajshahi", "রাজশাহী", "Boalia", "বোয়ালিয়া",
                        "Shaheb Bazar", "সাহেব বাজার", "6100"),
                    ("Bangladesh", "বাংলাদেশ", "Rajshahi", "রাজশাহী", "Natore", "নাটোর", "Natore Sadar", "নাটোর সদর",
                        "Natore", "নাটোর", "6400"),
                    ("Bangladesh", "বাংলাদেশ", "Khulna", "খুলনা", "Khulna", "খুলনা", "Daulatpur", "ডৌলতপুর",
                        "Daulatpur", "ডৌলতপুর", "9203"),
                    ("Bangladesh", "বাংলাদেশ", "Khulna", "খুলনা", "Jessore", "যশোর", "Jessore Sadar", "যশোর সদর",
                        "Jessore", "যশোর", "7400"),
                    ("Bangladesh", "বাংলাদেশ", "Sylhet", "সিলেট", "Sylhet", "সিলেট", "Sylhet Sadar", "সিলেট সদর",
                        "Zindabazar", "জিন্দাবাজার", "3100"),
                    ("Bangladesh", "বাংলাদেশ", "Sylhet", "সিলেট", "Moulvibazar", "মৌলভীবাজার", "Moulvibazar Sadar",
                        "মৌলভীবাজার সদর", "Moulvibazar", "মৌলভীবাজার", "3200"),
                    ("Bangladesh", "বাংলাদেশ", "Barisal", "বরিশাল", "Barisal", "বরিশাল", "Barisal Sadar", "বরিশাল সদর",
                        "Barisal Sadar", "বরিশাল সদর", "8200"),
                    ("Bangladesh", "বাংলাদেশ", "Rangpur", "রংপুর", "Rangpur", "রংপুর", "Rangpur Sadar", "রংপুর সদর",
                        "Rangpur Sadar", "রংপুর সদর", "5400"),
                };

            int row = 2;
            foreach (var data in demoData)
            {
                worksheet.Cells[row, 1].Value = data.Item1;
                worksheet.Cells[row, 2].Value = data.Item2;
                worksheet.Cells[row, 3].Value = data.Item3;
                worksheet.Cells[row, 4].Value = data.Item4;
                worksheet.Cells[row, 5].Value = data.Item5;
                worksheet.Cells[row, 6].Value = data.Item6;
                worksheet.Cells[row, 7].Value = data.Item7;
                worksheet.Cells[row, 8].Value = data.Item8;
                worksheet.Cells[row, 9].Value = data.Item9;
                worksheet.Cells[row, 10].Value = data.Item10;
                worksheet.Cells[row, 11].Value = data.Item11;
                row++;
            }

            worksheet.Cells.AutoFitColumns();

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Address_Demo_Data.xlsx");
        }

        [HttpPost("import")]
        [Authorize(Roles = UserRoles.SuperAdmin + "," + UserRoles.Admin + "," + UserRoles.HrManager + "," +
                           UserRoles.ItOfficer)]
        public async Task<ActionResult<AddressImportResultDto>> ImportFromExcel(IFormFile file)
        {
            if (file.Length == 0)
                return BadRequest("Please upload a valid Excel file.");

            var result = new AddressImportResultDto();

            try
            {
                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Position = 0;

                using var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();

                if (worksheet == null)
                {
                    result.Errors.Add(new ImportErrorDto
                        { RowNumber = 0, Field = "File", Message = "No worksheet found in the Excel file." });
                    return BadRequest(result);
                }

                var rowCount = worksheet.Dimension?.Rows ?? 0;
                if (rowCount <= 1)
                {
                    result.Errors.Add(new ImportErrorDto
                        { RowNumber = 0, Field = "File", Message = "No data rows found." });
                    return BadRequest(result);
                }

                result.TotalRows = rowCount - 1;

                for (int row = 2; row <= rowCount; row++)
                {
                    try
                    {
                        var countryNameEn = worksheet.Cells[row, 1].Text?.Trim();
                        var countryNameBn = worksheet.Cells[row, 2].Text?.Trim();
                        var divisionNameEn = worksheet.Cells[row, 3].Text?.Trim();
                        var divisionNameBn = worksheet.Cells[row, 4].Text?.Trim();
                        var districtNameEn = worksheet.Cells[row, 5].Text?.Trim();
                        var districtNameBn = worksheet.Cells[row, 6].Text?.Trim();
                        var thanaNameEn = worksheet.Cells[row, 7].Text?.Trim();
                        var thanaNameBn = worksheet.Cells[row, 8].Text?.Trim();
                        var poNameEn = worksheet.Cells[row, 9].Text?.Trim();
                        var poNameBn = worksheet.Cells[row, 10].Text?.Trim();
                        var postCode = worksheet.Cells[row, 11].Text?.Trim();

                        if (string.IsNullOrEmpty(countryNameEn)) continue;

                        // Country
                        var country = await _context.Countries.FirstOrDefaultAsync(c => c.NameEn == countryNameEn);
                        if (country == null)
                        {
                            country = new Country { NameEn = countryNameEn, NameBn = countryNameBn ?? "" };
                            _context.Countries.Add(country);
                            await _context.SaveChangesAsync();
                            result.CreatedCount++;
                        }

                        if (!string.IsNullOrEmpty(divisionNameEn))
                        {
                            var division = await _context.Divisions.FirstOrDefaultAsync(d =>
                                d.NameEn == divisionNameEn && d.CountryId == country.Id);
                            if (division == null)
                            {
                                division = new Division
                                    { NameEn = divisionNameEn, NameBn = divisionNameBn ?? "", CountryId = country.Id };
                                _context.Divisions.Add(division);
                                await _context.SaveChangesAsync();
                                result.CreatedCount++;
                            }

                            if (!string.IsNullOrEmpty(districtNameEn))
                            {
                                var district = await _context.Districts.FirstOrDefaultAsync(d =>
                                    d.NameEn == districtNameEn && d.DivisionId == division.Id);
                                if (district == null)
                                {
                                    district = new District
                                    {
                                        NameEn = districtNameEn, NameBn = districtNameBn ?? "", DivisionId = division.Id
                                    };
                                    _context.Districts.Add(district);
                                    await _context.SaveChangesAsync();
                                    result.CreatedCount++;
                                }

                                // Thana
                                if (!string.IsNullOrEmpty(thanaNameEn))
                                {
                                    var thana = await _context.Thanas.FirstOrDefaultAsync(t =>
                                        t.NameEn == thanaNameEn && t.DistrictId == district.Id);
                                    if (thana == null)
                                    {
                                        thana = new Thana
                                        {
                                            NameEn = thanaNameEn, NameBn = thanaNameBn ?? "", DistrictId = district.Id
                                        };
                                        _context.Thanas.Add(thana);
                                        await _context.SaveChangesAsync();
                                        result.CreatedCount++;
                                    }
                                }

                                // Post Office
                                if (!string.IsNullOrEmpty(poNameEn))
                                {
                                    var po = await _context.PostOffices.FirstOrDefaultAsync(p =>
                                        p.NameEn == poNameEn && p.DistrictId == district.Id);
                                    if (po == null)
                                    {
                                        po = new PostOffice
                                        {
                                            NameEn = poNameEn, NameBn = poNameBn ?? "", Code = postCode ?? "",
                                            DistrictId = district.Id
                                        };
                                        _context.PostOffices.Add(po);
                                        await _context.SaveChangesAsync();
                                        result.CreatedCount++;
                                    }
                                    else if (!string.IsNullOrEmpty(postCode))
                                    {
                                        po.Code = postCode;
                                        await _context.SaveChangesAsync();
                                        result.UpdatedCount++;
                                    }
                                }
                            }
                        }

                        result.SuccessCount++;
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add(new ImportErrorDto
                            { RowNumber = row, Field = "General", Message = ex.Message });
                        result.ErrorCount++;
                    }
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                result.Errors.Add(new ImportErrorDto
                    { RowNumber = 0, Field = "File", Message = $"Failed to process file: {ex.Message}" });
                return BadRequest(result);
            }
        }
    }
}
