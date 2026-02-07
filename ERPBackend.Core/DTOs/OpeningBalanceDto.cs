using System;

namespace ERPBackend.Core.DTOs
{
    public class OpeningBalanceDto
    {
        public int Id { get; set; }
        public required string AccountName { get; set; }
        public required string Category { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string? Remarks { get; set; }
    }
}
