using eShopLearning.Products.EFCoreRepositories.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.Products.EFCoreRepositories.Entities
{
    public class Category : SoftDeleteEntity<string>
    {
        /// <summary>
        /// 父级分类
        /// </summary>        
        public string ParentId { get; set; } = "";
        /// <summary>
        /// 类别名称
        /// </summary>
        [Required]
        public string Name { get; set; }
        /// <summary>
        /// 图片
        /// </summary>
        public string Image { get; set; } = "";
        /// <summary>
        /// 排序
        /// </summary>
        public int Order { get; set; } = 0;
    }
}
