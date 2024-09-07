﻿using Duha.SIMS.ServiceModels.Base;
using Duha.SIMS.ServiceModels.Enums;

namespace Duha.SIMS.ServiceModels.Product
{
    public class ProductCategorySM : SIMSServiceModelBase<int>
    {
        public string Name { get; set; }
        public string? Description { get; set; }

        public CategoryStatusSM Status { get; set; }
        public int? ProductCount { get; set; }
    }
}
