using eShopLearning.Products.EFCoreRepositories.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.Products.EFCoreRepositories.Entities
{
    /// <summary>
    /// SKU 
    /// </summary>
    public class Sku : SoftDeleteEntity<string>
    {
        /// <summary>
        /// sku展示给用户看的标题
        /// </summary>
        [Required]
        public string Title { get; set; }
        /// <summary>
        /// SPU ID
        /// </summary>
        public string SpuId { get; set; }
        /// <summary>
        /// sku的外观图片（轮播图,多张图使用 , 分割）
        /// </summary>
        public string RotatePictures { get; set; } = "";
        /// <summary>
        /// 详情（图片集，多张使用 , 分割）
        /// </summary>
        public string DetailContent { get; set; } = "";
        /// <summary>
        /// 价格
        /// </summary>
        [Required]
        public decimal Price { get; set; }
        /// <summary>
        /// 原价
        /// </summary>
        public decimal? OriginalPrice { get; set; } = null;
        /// <summary>
        /// 库存
        /// </summary>
        [Required]
        public int Inventory { get; set; }
    }
}
