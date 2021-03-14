using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.Users.EFCoreRepositories.Base
{
    /// <summary>
    /// 具备软删除实现的实体
    /// </summary>
    public class SoftDeleteEntity<TKey> : Entity<TKey>
    {
        /// <summary>
        /// 该记录的状态
        /// true : 未删除  false : 已删除
        /// </summary>
        public bool Status { get; set; } = true;
    }
}
