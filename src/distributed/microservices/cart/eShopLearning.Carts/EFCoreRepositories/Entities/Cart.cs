using eShopLearning.Carts.EFCoreRepositories.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.Carts.EFCoreRepositories.Entities
{
    public class Cart:Entity<long>
    {
        /// <summary>
        /// 用户id
        /// </summary>
        public long UserId { get; set; }
        /// <summary>
        /// 产品sku id
        /// </summary>
        public string SkuId { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public int Quantity { get; set; }
    }
}
