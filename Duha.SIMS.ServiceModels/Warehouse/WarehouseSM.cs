using Duha.SIMS.ServiceModels.Base;
using Duha.SIMS.ServiceModels.Enums;

namespace Duha.SIMS.ServiceModels.Warehouse
{
    public class WarehouseSM : SIMSServiceModelBase<int>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string ContactNumber { get; set; }
        public string EmailId { get; set; }
        public StorageTypeSM StorageType { get; set; }
        public int Capacity { get; set; }
        public bool IsActive { get; set; }
        public int? ClientCompanyDetailId { get; set; }
    }
}
