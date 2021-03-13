using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.IdentityAuth.Dto
{
    public class UserInfoDto
    {
        /// <summary>
        /// 主键id
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// 昵称
        /// </summary>
        public string NickName { get; set; } = "";
        /// <summary>
        /// 头像
        /// </summary>
        public string Avatar { get; set; } = "";
        /// <summary>
        /// 账户是否可用
        /// </summary>
        public bool Enable { get; set; } = true;
    }
}
