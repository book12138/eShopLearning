using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.Products.Dto
{
    public class SkuDto
    {
        /// <summary>
        /// sku展示给用户看的标题
        /// </summary>
        [Required]
        public string Title { get; set; }
        /// <summary>
        /// sku的外观图片（轮播图,多张图使用 , 分割）
        /// </summary>
        public IEnumerable<string> RotatePictures { get; set; }
        /// <summary>
        /// 详情（图片集，多张使用 , 分割）
        /// </summary>
        public IEnumerable<string> DetailContent { get; set; }
        /// <summary>
        /// 价格
        /// </summary>
        [Required]
        public decimal Price { get; set; }
        /// <summary>
        /// 库存
        /// </summary>
        [Required]
        public int Inventory { get; set; }
        /// <summary>
        /// sku 属性集合
        /// </summary>
        public IEnumerable<SkuAttrDto> SkuAttrs { get; set; }
    }
}
