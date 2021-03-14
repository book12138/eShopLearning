using eShopLearning.Users.EFCoreRepositories.Base;
using eShopLearning.Users.EFCoreRepositories.EFCore;
using eShopLearning.Users.EFCoreRepositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.Users.EFCoreRepositories.Repositories.Impl
{
    public class UserRepository : Repository<User, long> , IUserRepository
    {
        public UserRepository(ApplicationUserDbContext context)
            : base(context)
        { }
    }
}
