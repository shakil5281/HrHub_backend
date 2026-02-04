using System;

namespace ERPBackend.Core.DTOs
{
    public class TransferDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;

        public int? FromDepartmentId { get; set; }
        public string? FromDepartmentName { get; set; }
        public int? FromDesignationId { get; set; }
        public string? FromDesignationName { get; set; }

        public int ToDepartmentId { get; set; }
        public string? ToDepartmentName { get; set; }
        public int ToDesignationId { get; set; }
        public string? ToDesignationName { get; set; }

        public DateTime TransferDate { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; }
    }

    public class CreateTransferDto
    {
        public int EmployeeId { get; set; }
        public int ToDepartmentId { get; set; }
        public int ToDesignationId { get; set; }
        public DateTime TransferDate { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public class UpdateTransferStatusDto
    {
        public string Status { get; set; } = string.Empty;
        public string? AdminRemark { get; set; }
    }
}
