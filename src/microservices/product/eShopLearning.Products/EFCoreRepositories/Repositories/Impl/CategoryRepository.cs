using eShopLearning.Products.EFCoreRepositories.Base;
using eShopLearning.Products.EFCoreRepositories.EFCore;
using eShopLearning.Products.EFCoreRepositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.Users.EFCoreRepositories.Repositories.Impl
{
    public class CategoryRepository : Repository<Category, string> , ICategoryRepository
    {
        public CategoryRepository(eShopProductDbContext context)
            : base(context)
        { }
    }
}
