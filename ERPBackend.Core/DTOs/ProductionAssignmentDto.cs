using System;
using System.Collections.Generic;

namespace ERPBackend.Core.DTOs
{
    public class ProductionAssignmentDto
    {
        public int Id { get; set; }
        public int ProductionId { get; set; }
        public string StyleNo { get; set; } = string.Empty;
        public string Buyer { get; set; } = string.Empty;
        public int LineId { get; set; }
        public string LineName { get; set; } = string.Empty;
        public int TotalTarget { get; set; }
        public DateTime AssignDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class UpdateProductionAssignmentDto
    {
        public int ProductionId { get; set; }
        public int LineId { get; set; }
        public int TotalTarget { get; set; }
        public string Status { get; set; } = "Active";
    }

    public class CreateProductionAssignmentDto
    {
        public int ProductionId { get; set; }
        public int LineId { get; set; }
        public int TotalTarget { get; set; }
        public string Status { get; set; } = "Active";
    }

    public class DailyProductionRecordDto
    {
        public int Id { get; set; }
        public int AssignmentId { get; set; }
        public DateTime Date { get; set; }
        public int DailyTarget { get; set; }
        public int HourlyTarget { get; set; }
        public int H1 { get; set; }
        public int H2 { get; set; }
        public int H3 { get; set; }
        public int H4 { get; set; }
        public int H5 { get; set; }
        public int H6 { get; set; }
        public int H7 { get; set; }
        public int H8 { get; set; }
        public int H9 { get; set; }
        public int H10 { get; set; }
        public int H11 { get; set; }
        public int H12 { get; set; }
        public int H13 { get; set; }
        public int H14 { get; set; }
        public int H15 { get; set; }
        public int H16 { get; set; }
        public int H17 { get; set; }
        public int H18 { get; set; }
        public int TotalCompleted { get; set; }
    }

    public class SaveDailyProductionDto
    {
        public int AssignmentId { get; set; }
        public DateTime Date { get; set; }
        public int DailyTarget { get; set; }
        public int HourlyTarget { get; set; }
        public int H1 { get; set; }
        public int H2 { get; set; }
        public int H3 { get; set; }
        public int H4 { get; set; }
        public int H5 { get; set; }
        public int H6 { get; set; }
        public int H7 { get; set; }
        public int H8 { get; set; }
        public int H9 { get; set; }
        public int H10 { get; set; }
        public int H11 { get; set; }
        public int H12 { get; set; }
        public int H13 { get; set; }
        public int H14 { get; set; }
        public int H15 { get; set; }
        public int H16 { get; set; }
        public int H17 { get; set; }
        public int H18 { get; set; }
    }

    public class ProductionFilterDto
    {
        public DateTime? Date { get; set; }
        public int? LineId { get; set; }
        public string? Buyer { get; set; }
        public string? StyleNo { get; set; }
        public string? SearchTerm { get; set; }
    }

    public class DailyReportItemDto
    {
        public int AssignmentId { get; set; }
        public string LineName { get; set; } = "";
        public string StyleNo { get; set; } = "";
        public string Buyer { get; set; } = "";
        public int DailyTarget { get; set; }
        public int HourlyTarget { get; set; }
        public int Completed { get; set; }
        public double Achievement { get; set; }
    }

    public class MonthlyReportItemDto
    {
        public string Month { get; set; } = "";
        public int Year { get; set; }
        public string LineName { get; set; } = "";
        public int TotalTarget { get; set; }
        public int TotalCompleted { get; set; }
        public double AvgAchievement { get; set; }
        public int WorkingDays { get; set; }
        public string TopStyle { get; set; } = "";
    }
}
