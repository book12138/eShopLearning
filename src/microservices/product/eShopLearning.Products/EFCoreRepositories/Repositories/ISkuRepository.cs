using eShopLearning.Products.EFCoreRepositories.Base;
using eShopLearning.Products.EFCoreRepositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.Users.EFCoreRepositories.Repositories
{
    public interface ISkuRepository : IRepository<Sku, string>
    {
    }
}
