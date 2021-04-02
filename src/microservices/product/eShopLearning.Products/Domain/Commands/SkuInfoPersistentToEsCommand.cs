using eShopLearning.Products.EFCoreRepositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.Products.Domain.Commands
{
    public class SkuInfoPersistentToEsCommand : Base.DomainCommand
    {
        public IEnumerable<Sku> Skus { get; set; }
        public SkuInfoPersistentToEsCommand(IEnumerable<Sku> skus) => Skus = skus;
    }
}
