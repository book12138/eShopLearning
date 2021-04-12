using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.WapAggregator.Dto
{
    /// <summary>
    /// sku 基础信息
    /// </summary>
    public class SkuBasicInfo
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 主图
        /// </summary>
        public string Image { get; set; }
        /// <summary>
        /// 价格
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// sku 属性集
        /// </summary>
        public string[] SkuAttrs { get; set; }        
    }
}
