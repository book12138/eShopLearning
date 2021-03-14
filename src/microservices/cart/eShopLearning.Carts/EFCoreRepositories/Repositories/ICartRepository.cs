using eShopLearning.Carts.EFCoreRepositories.Base;
using eShopLearning.Carts.EFCoreRepositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.Carts.EFCoreRepositories.Repositories
{
    public interface ICartRepository : IRepository<Cart, long>
    {
    }
}
