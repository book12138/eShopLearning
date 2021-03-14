using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.Users.EFCoreRepositories.Base
{
    /// <summary>
    /// 实体基类
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class Entity<TKey>
    {
        /// <summary>
        /// 主键id
        /// </summary>
        [Key]
        public TKey Id { get; set; }
        /// <summary>
        /// 记录创建时间
        /// </summary>
        public DateTime AddTime { get; set; }
        /// <summary>
        /// 此记录上一次的修改时间
        /// </summary>
        public DateTime UpdateTime { get; set; }
    }
}
