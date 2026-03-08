namespace ERPBackend.Core.DTOs
{
    public class MasterDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class FabricTypeGsmDto : MasterDto
    {
        public string FabricType { get; set; } = string.Empty;
        public string Gsm { get; set; } = string.Empty;
    }

    public class SupplierInfoDto : MasterDto
    {
        public string SupplierName { get; set; } = string.Empty;
        public string SupplierType { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
