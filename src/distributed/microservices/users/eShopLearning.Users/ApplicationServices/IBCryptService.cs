using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.Users.ApplicationServices
{
    /// <summary>
    /// 加密服务
    /// </summary>
    public interface IBCryptService
    {
        /// <summary>
        /// 密码哈希
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        string HashPassword(string password);
        /// <summary>
        /// 明文与哈希值进行核对
        /// </summary>
        /// <param name="cleartext">明文</param>
        /// <param name="hash">哈希值</param>
        /// <returns></returns>
        bool Verify(string cleartext, string hash);
    }
}
