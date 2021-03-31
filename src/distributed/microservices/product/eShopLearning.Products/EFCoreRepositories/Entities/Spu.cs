using eShopLearning.Products.EFCoreRepositories.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.Products.EFCoreRepositories.Entities
{
    /// <summary>
    /// SPU
    /// </summary>
    public class Spu : SoftDeleteEntity<string>
    {
        /// <summary>
        /// 商品所属分类
        /// </summary>
        public string Category { get; set; } 
    }
}
