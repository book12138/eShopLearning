using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.Users.ApplicationServices
{
    public interface IIdentityService
    {
        /// <summary>
        /// 提取用户身份标识
        /// </summary>
        /// <returns></returns>
        string GetUserIdentity();
    }
}
