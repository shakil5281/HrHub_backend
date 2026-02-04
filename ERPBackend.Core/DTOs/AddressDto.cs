namespace ERPBackend.Core.DTOs
{
    // --- Country ---
    public class CountryDto
    {
        public int Id { get; set; }
        public string NameEn { get; set; } = string.Empty;
        public string? NameBn { get; set; }
    }

    public class CreateCountryDto
    {
        public string NameEn { get; set; } = string.Empty;
        public string? NameBn { get; set; }
    }

    // --- Division ---
    public class DivisionDto
    {
        public int Id { get; set; }
        public string NameEn { get; set; } = string.Empty;
        public string? NameBn { get; set; }
        public int CountryId { get; set; }
        public string? CountryName { get; set; }
    }

    public class CreateDivisionDto
    {
        public string NameEn { get; set; } = string.Empty;
        public string? NameBn { get; set; }
        public int CountryId { get; set; }
    }

    // --- District ---
    public class DistrictDto
    {
        public int Id { get; set; }
        public string NameEn { get; set; } = string.Empty;
        public string? NameBn { get; set; }
        public int DivisionId { get; set; }
        public string? DivisionName { get; set; }
    }

    public class CreateDistrictDto
    {
        public string NameEn { get; set; } = string.Empty;
        public string? NameBn { get; set; }
        public int DivisionId { get; set; }
    }

    // --- Thana ---
    public class ThanaDto
    {
        public int Id { get; set; }
        public string NameEn { get; set; } = string.Empty;
        public string? NameBn { get; set; }
        public int DistrictId { get; set; }
        public string? DistrictName { get; set; }
    }

    public class CreateThanaDto
    {
        public string NameEn { get; set; } = string.Empty;
        public string? NameBn { get; set; }
        public int DistrictId { get; set; }
    }

    // --- Post Office ---
    public class PostOfficeDto
    {
        public int Id { get; set; }
        public required string NameEn { get; set; }
        public string? NameBn { get; set; }
        public required string Code { get; set; }
        public int DistrictId { get; set; }
        public string? DistrictName { get; set; }
    }

    public class CreatePostOfficeDto
    {
        public required string NameEn { get; set; }
        public string? NameBn { get; set; }
        public required string Code { get; set; }
        public int DistrictId { get; set; }
    }
}
