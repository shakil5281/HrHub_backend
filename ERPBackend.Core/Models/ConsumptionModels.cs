using System.Collections.Generic;
using System.Threading.Tasks;

namespace ERPBackend.Core.Models
{
    public class FabricConsumption : MerchandisingBase
    {
        public int StyleId { get; set; }
        public int ItemId { get; set; }
        public string ComponentName { get; set; } = "Body";
        public decimal ConsumptionPerPcs { get; set; }
        public decimal WastagePercentage { get; set; }
        public decimal TotalConsumption { get; set; }
        public string Unit { get; set; } = "Kg";
    }

    public class AccessoriesConsumption : MerchandisingBase
    {
        public int StyleId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public decimal ConsumptionPerPcs { get; set; }
        public decimal WastagePercentage { get; set; }
    }
}
