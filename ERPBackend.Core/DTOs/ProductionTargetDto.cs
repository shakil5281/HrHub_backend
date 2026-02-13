using System;

namespace ERPBackend.Core.DTOs
{
    public class ProductionTargetDto
    {
        public int Id { get; set; }
        public int AssignmentId { get; set; }
        public string StyleNo { get; set; } = string.Empty;
        public string LineName { get; set; } = string.Empty;
        public string Buyer { get; set; } = string.Empty;
        public DateTime TargetDate { get; set; }
        public int DailyTarget { get; set; }
        public int HourlyTarget { get; set; }
        public string Remarks { get; set; } = string.Empty;
    }

    public class CreateProductionTargetDto
    {
        public int AssignmentId { get; set; }
        public DateTime TargetDate { get; set; }
        public int DailyTarget { get; set; }
        public int HourlyTarget { get; set; }
        public string Remarks { get; set; } = string.Empty;
    }
}
