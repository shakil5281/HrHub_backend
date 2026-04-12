using System.Text.Json.Serialization;

namespace ERPBackend.Core.DTOs
{
    public class MonthlySalarySheetDto
    {
        public int Id { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public decimal GrossSalary { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal HouseRent { get; set; }
        public decimal MedicalAllowance { get; set; }
        public decimal FoodAllowance { get; set; }
        public decimal Conveyance { get; set; }
        public int TotalDays { get; set; }
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public int LeaveDays { get; set; }
        public int Holidays { get; set; }
        public int WeekendDays { get; set; }
        public decimal OTHours { get; set; }
        public decimal OTRate { get; set; }
        public decimal OTAmount { get; set; }
        public decimal AttendanceBonus { get; set; }
        public decimal OtherAllowances { get; set; }
        public decimal TotalEarning { get; set; }
        public decimal AbsentDeduction { get; set; }
        public decimal TotalDeduction { get; set; }
        public decimal NetPayable { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? CompanyName { get; set; }
        public string? JoinedDate { get; set; }
        public string? BankAccountNo { get; set; }
    }

    public class DailySalarySheetDto
    {
        public int Id { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal PerDaySalary { get; set; }
        public string AttendanceStatus { get; set; } = string.Empty;
        public decimal OTHours { get; set; }
        public decimal OTAmount { get; set; }
        public decimal TotalEarning { get; set; }
        public decimal Deduction { get; set; }
        public decimal NetPayable { get; set; }
        public string? CompanyName { get; set; }
    }

    public class SalarySummaryDto
    {
        [JsonPropertyName("totalGrossSalary")]
        public decimal TotalGrossSalary { get; set; }

        [JsonPropertyName("totalOTAmount")]
        public decimal TotalOTAmount { get; set; }

        [JsonPropertyName("totalDeductions")]
        public decimal TotalDeductions { get; set; }

        [JsonPropertyName("totalNetPayable")]
        public decimal TotalNetPayable { get; set; }

        [JsonPropertyName("totalEmployees")]
        public int TotalEmployees { get; set; }

        [JsonPropertyName("departmentSummaries")]
        public List<SalarySummaryItemDto> DepartmentSummaries { get; set; } = new();

        [JsonPropertyName("sectionSummaries")]
        public List<SalarySummaryItemDto> SectionSummaries { get; set; } = new();

        [JsonPropertyName("lineSummaries")]
        public List<SalarySummaryItemDto> LineSummaries { get; set; } = new();

        [JsonPropertyName("groupSummaries")]
        public List<SalarySummaryItemDto> GroupSummaries { get; set; } = new();
    }

    public class SalarySummaryItemDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("totalGrossSalary")]
        public decimal TotalGrossSalary { get; set; }

        [JsonPropertyName("totalOTAmount")]
        public decimal TotalOTAmount { get; set; }

        [JsonPropertyName("totalDeductions")]
        public decimal TotalDeductions { get; set; }

        [JsonPropertyName("totalNetPayable")]
        public decimal TotalNetPayable { get; set; }

        [JsonPropertyName("employeeCount")]
        public int EmployeeCount { get; set; }
    }

    public class SalaryProcessRequestDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int? DepartmentId { get; set; }
        public string? EmployeeId { get; set; }
        public int? CompanyId { get; set; }
    }

    public class PayslipDto
    {
        public int Id { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public decimal GrossSalary { get; set; }
        public decimal BasicSalary { get; set; }
        public int TotalDays { get; set; }
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public int LeaveDays { get; set; }
        public int Holidays { get; set; }
        public int WeekendDays { get; set; }
        public decimal OTHours { get; set; }
        public decimal OTAmount { get; set; }
        public decimal AttendanceBonus { get; set; }
        public decimal OtherAllowances { get; set; }
        public decimal TotalEarning { get; set; }
        public decimal TotalDeduction { get; set; }
        public decimal NetPayable { get; set; }
        public string Status { get; set; } = string.Empty;
        public string JoinedDate { get; set; } = string.Empty;
        public string BankAccountNo { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = "Bank";
        public decimal Arrears { get; set; }
        public decimal TaxDeduction { get; set; }
        public decimal PFContribution { get; set; }
        public string? CompanyName { get; set; }
    }

    public class AdvanceSalaryDto
    {
        public int Id { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public DateTime? JoiningDate { get; set; }
        public string? Grade { get; set; }
        public decimal Amount { get; set; }
        public DateTime RequestDate { get; set; }
        public int RepaymentMonth { get; set; }
        public int RepaymentYear { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public string? CompanyName { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal HouseRent { get; set; }
        public decimal MedicalAllowance { get; set; }
        public decimal FoodAllowance { get; set; }
        public decimal TransportAllowance { get; set; }
        public decimal GrossSalary { get; set; }
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public decimal AbsentDeduction { get; set; }
        public decimal TotalPayableWages { get; set; }
        public decimal OTHours { get; set; }
        public decimal OTRate { get; set; }
        public decimal OTAmount { get; set; }
        public string? BankAccountNo { get; set; }
        public string? PaymentMethod { get; set; }
        public decimal NetPayable { get; set; }
    }

    public class SalaryIncrementDto
    {
        public int Id { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public decimal PreviousGrossSalary { get; set; }
        public decimal IncrementAmount { get; set; }
        public decimal NewGrossSalary { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string IncrementType { get; set; } = string.Empty;
        public bool IsApplied { get; set; }
        public string? CompanyName { get; set; }
    }

    public class BonusDto
    {
        public int Id { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string BonusType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? CompanyName { get; set; }
        public DateTime? JoiningDate { get; set; }
        public decimal GrossSalary { get; set; }
        public string? JobAge { get; set; }
    }

    public class CreateAdvanceSalaryDto
    {
        public string EmployeeId { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public decimal Amount { get; set; }
        public DateTime RequestDate { get; set; }
        public int RepaymentMonth { get; set; }
        public int RepaymentYear { get; set; }
        public string? Remarks { get; set; }
    }

    public class CreateSalaryIncrementDto
    {
        public string EmployeeId { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public decimal IncrementAmount { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string IncrementType { get; set; } = "Yearly";
        public string? Remarks { get; set; }
    }

    public class CreateBonusDto
    {
        public string EmployeeId { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public string BonusType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
    }

    public class FestivalBonusProcessRequestDto
    {
        public string BonusType { get; set; } = string.Empty; // e.g. Eid-ul-Fitr, Eid-ul-Adha
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Percentage { get; set; } = 100; // % of base salary
        public string BaseOn { get; set; } = "Basic"; // "Basic" or "Gross"
        public int? CompanyId { get; set; }
    }

    public class FestivalBonusSummaryDto
    {
        public int ProcessedCount { get; set; }
        public int SkippedCount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class BankSheetDto
    {
        public int Id { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string BankAccountNo { get; set; } = string.Empty;
        public string BankBranchName { get; set; } = string.Empty;
        public decimal NetPayable { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string? CompanyName { get; set; }
    }

    public class DailyProcessRequestDto
    {
        public DateTime Date { get; set; }
        public int? CompanyId { get; set; }
        public int? DepartmentId { get; set; }
        public string? EmployeeId { get; set; }
    }

    public class DailyProcessResultDto
    {
        public int ProcessedCount { get; set; }
        public int SkippedCount { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class BatchCreateAdvanceSalaryDto
    {
        public List<string> EmployeeIds { get; set; } = new();
        public int CompanyId { get; set; }
        public decimal Amount { get; set; }
        public bool IsDateRange { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public DateTime RequestDate { get; set; }
        public int RepaymentMonth { get; set; }
        public int RepaymentYear { get; set; }
        public string? Remarks { get; set; }
    }

    public class AdvanceSalarySummaryDto
    {
        public decimal TotalAdvanceDisbursed { get; set; }
        public int TotalPendingRequests { get; set; }
        public decimal TotalPendingAmount { get; set; }
        public decimal TotalRepaid { get; set; }
        public int TotalEmployees { get; set; }
        public List<DepartmentAdvanceSummaryDto> DepartmentSummaries { get; set; } = new();
        public List<SectionAdvanceSummaryDto> SectionSummaries { get; set; } = new();
        public List<LineAdvanceSummaryDto> LineSummaries { get; set; } = new();
        public List<DesignationAdvanceSummaryDto> DesignationSummaries { get; set; } = new();
    }

    public class DepartmentAdvanceSummaryDto
    {
        public string DepartmentName { get; set; } = string.Empty;
        public int EmployeeCount { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal AbsentDays { get; set; }
        public decimal AbsentDeduction { get; set; }
        public decimal TotalPayableWages { get; set; }
        public decimal OTHours { get; set; }
        public decimal OTAmount { get; set; }
        public decimal NetPayable { get; set; }
    }

    public class SectionAdvanceSummaryDto
    {
        public string SectionName { get; set; } = string.Empty;
        public int EmployeeCount { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal AbsentDays { get; set; }
        public decimal AbsentDeduction { get; set; }
        public decimal TotalPayableWages { get; set; }
        public decimal OTHours { get; set; }
        public decimal OTAmount { get; set; }
        public decimal NetPayable { get; set; }
    }

    public class LineAdvanceSummaryDto
    {
        public string LineName { get; set; } = string.Empty;
        public int EmployeeCount { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal AbsentDays { get; set; }
        public decimal AbsentDeduction { get; set; }
        public decimal TotalPayableWages { get; set; }
        public decimal OTHours { get; set; }
        public decimal OTAmount { get; set; }
        public decimal NetPayable { get; set; }
    }

    public class DesignationAdvanceSummaryDto
    {
        public string DesignationName { get; set; } = string.Empty;
        public int EmployeeCount { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal AbsentDays { get; set; }
        public decimal AbsentDeduction { get; set; }
        public decimal TotalPayableWages { get; set; }
        public decimal OTHours { get; set; }
        public decimal OTAmount { get; set; }
        public decimal NetPayable { get; set; }
    }
}

