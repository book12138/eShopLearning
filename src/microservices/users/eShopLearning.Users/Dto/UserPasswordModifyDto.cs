using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.Users.Dto
{
    public class UserPasswordModifyDto
    {
        /// <summary>
        /// 用户唯一标识
        /// </summary>
        public long UserId { get; set; }
        /// <summary>
        /// 原密码
        /// </summary>
        [Required]
        public string OriginalPassword { get; set; }
        /// <summary>
        /// 新密码
        /// </summary>
        [Required]
        public string NewPassword { get; set; }
    }
}