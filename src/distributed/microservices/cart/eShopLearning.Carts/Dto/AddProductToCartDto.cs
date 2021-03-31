using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.Carts.Dto
{
    public class AddProductToCartDto
    {
        /// <summary>
        /// 产品id
        /// </summary>
        public string SkuId { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public int Quantity { get; set; }
    }
}
