using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.Products.Dto
{
    public class SkuAttrDto
    {
        /// <summary>
        /// 属性类型
        /// </summary>        
        public string Type { get; set; } = "";
        /// <summary>
        /// 属性名
        /// </summary>
        public string Name { get; set; } = "";
        /// <summary>
        /// 关于此属性的图片
        /// </summary>
        public string Image { get; set; } = "";
    }
}
