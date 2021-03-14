using eShopLearning.Users.EFCoreRepositories.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.Users.EFCoreRepositories.Entities
{
    public class User : SoftDeleteEntity<long>
    {
        /// <summary>
        /// 用户名
        /// </summary>
        [Required]
        public string Username { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        [Required]
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
