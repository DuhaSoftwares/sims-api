using Duha.SIMS.DomainModels.Base;
using Duha.SIMS.DomainModels.Client;
using Duha.SIMS.DomainModels.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Duha.SIMS.DomainModels.Warehouse
{
    public class WarehouseDM : SIMSDomainModelBase<int>
    {
        public string Name  { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string ContactNumber { get; set; }
        public string EmailId { get; set; }
        public StorageTypeDM StorageType { get; set; }
        public int Capacity { get; set; }
        public bool IsActive { get; set; }
        
        [ForeignKey(nameof(ClientCompanyDetail))]
        public int? ClientCompanyDetailId { get; set; }
        public virtual ClientCompanyDetailDM ClientCompanyDetail { get; set; }
    }
}
