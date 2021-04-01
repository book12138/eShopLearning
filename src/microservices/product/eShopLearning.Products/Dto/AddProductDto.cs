using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.Products.Dto
{
    public class AddProductDto
    {
        /// <summary>
        /// SKU 集合
        /// </summary>
        public IEnumerable<SkuDto> Skus { get; set; }
        /// <summary>
        /// 所属分类
        /// </summary>
        public string Category { get; set; }
    }
}
