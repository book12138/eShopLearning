using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityAuth.Dto
{
    /// <summary>
    /// 密码核对结果
    /// 当密码准确时，还会包含用户的一些详细信息
    /// </summary>
    public class UserPasswordCheckResultWithUserInfoDto
    {
        /// <summary>
        /// 密码是否是准确的
        /// </summary>
        public bool IsTrue { get; set; }
        /// <summary>
        /// 用户信息，当密码准确时才会被填充内容
        /// </summary>
        public UserInfoDto UserInfo { get; set; }
    }
}
