using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.Users.ApplicationServices.Impl
{
    /// <summary>
    /// 加密服务
    /// </summary>
    public class BCryptService : IBCryptService
    {
        /// <summary>
        /// 密码哈希
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public string HashPassword(string password)
            => BCrypt.Net.BCrypt.EnhancedHashPassword(password.Trim(), BCrypt.Net.HashType.SHA384);
        /// <summary>
        /// 明文与哈希值进行核对
        /// </summary>
        /// <param name="cleartext">明文</param>
        /// <param name="hash">哈希值</param>
        /// <returns></returns>
        public bool Verify(string cleartext, string hash)
            => BCrypt.Net.BCrypt.EnhancedVerify(cleartext, hash, BCrypt.Net.HashType.SHA384);
    }
}
