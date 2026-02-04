namespace ERPBackend.Core.DTOs
{
    public class MonthlySalarySheetDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeIdCard { get; set; } = string.Empty;
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
    }

    public class DailySalarySheetDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeIdCard { get; set; } = string.Empty;
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
    }

    public class SalarySummaryDto
    {
        public decimal TotalGrossSalary { get; set; }
        public decimal TotalOTAmount { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal TotalNetPayable { get; set; }
        public int TotalEmployees { get; set; }
        public List<DepartmentSalarySummaryDto> DepartmentSummaries { get; set; } = new();
    }

    public class DepartmentSalarySummaryDto
    {
        public string DepartmentName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int EmployeeCount { get; set; }
    }

    public class SalaryProcessRequestDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int? DepartmentId { get; set; }
        public int? EmployeeId { get; set; }
    }

    public class PayslipDto : MonthlySalarySheetDto
    {
        public string JoinedDate { get; set; } = string.Empty;
        public string BankAccountNo { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = "Bank";
        public decimal Arrears { get; set; }
        public decimal TaxDeduction { get; set; }
        public decimal PFContribution { get; set; }
    }
    public class AdvanceSalaryDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeIdCard { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime RequestDate { get; set; }
        public int RepaymentMonth { get; set; }
        public int RepaymentYear { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Remarks { get; set; }
    }

    public class SalaryIncrementDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeIdCard { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public decimal PreviousGrossSalary { get; set; }
        public decimal IncrementAmount { get; set; }
        public decimal NewGrossSalary { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string IncrementType { get; set; } = string.Empty;
        public bool IsApplied { get; set; }
    }

    public class BonusDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeIdCard { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string BonusType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class CreateAdvanceSalaryDto
    {
        public int EmployeeId { get; set; }
        public decimal Amount { get; set; }
        public DateTime RequestDate { get; set; }
        public int RepaymentMonth { get; set; }
        public int RepaymentYear { get; set; }
        public string? Remarks { get; set; }
    }

    public class CreateSalaryIncrementDto
    {
        public int EmployeeId { get; set; }
        public decimal IncrementAmount { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string IncrementType { get; set; } = "Yearly";
        public string? Remarks { get; set; }
    }

    public class CreateBonusDto
    {
        public int EmployeeId { get; set; }
        public string BonusType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
    }
}
