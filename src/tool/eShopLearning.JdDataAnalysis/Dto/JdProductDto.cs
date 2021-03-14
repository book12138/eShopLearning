using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopLearning.JdDataAnalysis.Dto
{
    public class JdProductDto
    {
        /// <summary>
        /// 商品标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 主图
        /// </summary>
        public string MasterImg { get; set; }
        /// <summary>
        /// 商品轮播图
        /// </summary>
        public List<string> CarouselImages { get; set; }
        /// <summary>
        /// 商品详情图
        /// </summary>
        public List<string> DetailImages { get; set; }
        /// <summary>
        /// 价格
        /// </summary>
        public decimal Price { get; set; }
    }
}
