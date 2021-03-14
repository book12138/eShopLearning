using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopLearning.JdDataAnalysis.Dto
{
    public class JdSkuDto
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 商品轮播图
        /// </summary>
        public List<string> RotatePictures { get; set; }
        /// <summary>
        /// 商品详情图
        /// </summary>
        public List<string> DetailContent { get; set; }
        /// <summary>
        /// 价格
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// 库存
        /// </summary>
        public int Inventory { get; set; }
        /// <summary>
        /// 属性集
        /// </summary>
        public List<JdSkuAttrDto> SkuAttrs { get; set; }


        /// <summary>
        /// 商品的属性
        /// </summary>
        public class JdSkuAttrDto
        {
            /// <summary>
            /// sku id (在此不存在任何意义，仅仅只是为了迎合 sku服务)
            /// </summary>
            //public long SkuId { get; set; }

            /// <summary>
            /// 属性类型
            /// </summary>
            public string Type { get; set; }
            /// <summary>
            /// 属性图片
            /// </summary>
            public string Image { get; set; }
            /// <summary>
            /// 属性名
            /// </summary>
            public string Name { get; set; }
        }
    }
}
