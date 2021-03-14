using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.Carts.Dto
{
    public class UserCartProductDto
    {
        /// <summary>
        /// 用户购物车中的记录id
        /// </summary>
        public string SkuId { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public int Quantity { get; set; }
    }
}
