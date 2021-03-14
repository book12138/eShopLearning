using eShopLearning.Products.EFCoreRepositories.Base;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.Products.EFCoreRepositories.Entities
{
    /// <summary>
    /// SKU 属性
    /// </summary>
    public class SkuAttr: SoftDeleteEntity<string>
    {
        /// <summary>
        /// SKU ID
        /// </summary>
        public string SkuId { get; set; }
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
