using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.Products.Dto
{
    public class EsSkuDto
    {
        /// <summary>
        /// sku展示给用户看的标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// SPU ID
        /// </summary>
        public long SpuId { get; set; }
        /// <summary>
        /// sku id
        /// </summary>
        public string SkuId { get; set; }
        /// <summary>
        /// 价格
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// 一张图
        /// </summary>
        public string Image { get; set; }
    }
}
