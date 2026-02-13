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
    }
}
