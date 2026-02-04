namespace ERPBackend.Core.DTOs
{
    public class ManpowerRequirementDto
    {
        public int Id { get; set; }
        public int DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public int DesignationId { get; set; }
        public string? DesignationName { get; set; }
        public int RequiredCount { get; set; }
        public int CurrentCount { get; set; }
        public int Gap { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateManpowerRequirementDto
    {
        public int DepartmentId { get; set; }
        public int DesignationId { get; set; }
        public int RequiredCount { get; set; }
        public string? Note { get; set; }
    }
}
