using eShopLearning.Users.EFCoreRepositories.Base;
using eShopLearning.Users.EFCoreRepositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.Users.EFCoreRepositories.Repositories
{
    public interface IUserRepository : IRepository<User, long>
    {
    }
}
