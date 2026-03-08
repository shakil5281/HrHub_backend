using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPBackend.Core.Models
{
    public abstract class MerchandisingBase
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // Master Setup Entities
    public class Season : MerchandisingBase
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class MerchandisingDepartment : MerchandisingBase
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;
    }

    public class LocalAgent : MerchandisingBase
    {
        [Required, StringLength(200)]
        public string Name { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class FabricColorPantone : MerchandisingBase
    {
        [Required, StringLength(100)]
        public string ColorName { get; set; } = string.Empty;
        public string PantoneCode { get; set; } = string.Empty;
    }

    public class FabricTypeGsm : MerchandisingBase
    {
        [Required, StringLength(100)]
        public string FabricType { get; set; } = string.Empty;
        public string Gsm { get; set; } = string.Empty;
    }

    public class ShipmentModeTerms : MerchandisingBase
    {
        [Required, StringLength(100)]
        public string Mode { get; set; } = string.Empty; // Sea, Air, Road
        public string Terms { get; set; } = string.Empty; // FOB, CNF, CIF
    }

    public class PaymentModeTerms : MerchandisingBase
    {
        [Required, StringLength(100)]
        public string Mode { get; set; } = string.Empty; // LC, TT, Cash
        public string Terms { get; set; } = string.Empty;
    }

    public class SupplierInfo : MerchandisingBase
    {
        [Required, StringLength(200)]
        public string SupplierName { get; set; } = string.Empty;
        public string SupplierType { get; set; } = string.Empty; // Yarn, Fabric, Accessories
        public string ContactPerson { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }

    public class CourierInfo : MerchandisingBase
    {
        [Required, StringLength(200)]
        public string CourierName { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
    }

    public class SizeName : MerchandisingBase
    {
        [Required, StringLength(50)]
        public string Name { get; set; } = string.Empty; // S, M, L, XL
        public int SortOrder { get; set; }
    }

    public class ExportItem : MerchandisingBase
    {
        [Required, StringLength(200)]
        public string ItemName { get; set; } = string.Empty;
        public string HSCode { get; set; } = string.Empty;
    }

    public class KnitMachine : MerchandisingBase
    {
        [Required, StringLength(100)]
        public string MachineNo { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Capacity { get; set; } = string.Empty;
        public string Gauge { get; set; } = string.Empty;
        public string Diameter { get; set; } = string.Empty;
    }

    public class DyeingMachine : MerchandisingBase
    {
        [Required, StringLength(100)]
        public string MachineNo { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Capacity { get; set; } = string.Empty;
    }

    public class YarnInventory : MerchandisingBase
    {
        public string YarnType { get; set; } = string.Empty;
        public string Count { get; set; } = string.Empty;
        public string Composition { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public decimal StockQty { get; set; }
        public string Unit { get; set; } = "Kg";
    }

    public class SubContractOrder : MerchandisingBase
    {
        public string OrderNo { get; set; } = string.Empty;
        public string PartyName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal Qty { get; set; }
        public string ProcessName { get; set; } = string.Empty; // Knit, Dye, Print
    }
}
