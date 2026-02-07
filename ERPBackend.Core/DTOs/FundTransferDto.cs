using System;

namespace ERPBackend.Core.DTOs
{
    public class FundTransferDto
    {
        public int Id { get; set; }
        public required string FromBranch { get; set; }
        public required string ToBranch { get; set; }
        public decimal RequestedAmount { get; set; }
        public decimal? ApprovedAmount { get; set; }
        public string? Reason { get; set; }
        public string? Status { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
    }
}
