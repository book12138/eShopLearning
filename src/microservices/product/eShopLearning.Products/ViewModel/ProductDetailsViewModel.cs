using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.Products.ViewModel
{
    /// <summary>
    /// 商品详情视图模型
    /// </summary>
    public class ProductDetailsViewModel
    {
        /// <summary>
        /// sku展示给用户看的标题
        /// </summary>
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
        public decimal Price { get; set; }
        /// <summary>
        /// 库存
        /// </summary>
        public int Inventory { get; set; }
        /// <summary>
        /// 属性分组集合
        /// </summary>
        public List<AttrSkuGroup> AttrSkuGroups { get; set; }

        /// <summary>
        /// 根据同类型属性进行SKU分组
        /// </summary>
        public class AttrSkuGroup
        {
            /// <summary>
            /// 属性分组名
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// 属性分组中，每种属性所代表的SKU集合
            /// </summary>
            public IEnumerable<AttrSku> AttrSkus { get; set; }

            /// <summary>
            /// 对应该属性组中，分别代表每种属性值的SKU
            /// </summary>
            public class AttrSku
            {
                /// <summary>
                /// SKU ID
                /// </summary>
                public string SkuId { get; set; }
                /// <summary>
                /// 属性名
                /// </summary>
                public string Name { get; set; }
                /// <summary>
                /// 属性图片
                /// </summary>
                public string Image { get; set; }
            }
        }
    }
}
