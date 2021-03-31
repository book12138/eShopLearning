using eShopLearning.Products.EFCoreRepositories.Base;
using eShopLearning.Products.EFCoreRepositories.EFCore;
using eShopLearning.Products.EFCoreRepositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.Users.EFCoreRepositories.Repositories.Impl
{
    public class SkuAttrRepository : Repository<SkuAttr, string> , ISkuAttrRepository
    {
        public SkuAttrRepository(eShopProductDbContext context)
            : base(context)
        { }
    }
}
