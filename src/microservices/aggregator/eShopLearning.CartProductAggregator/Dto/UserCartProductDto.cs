
namespace eShopLearning.CartProductAggregator.Dto
{
    public class UserCartProductDto
    {
        /// <summary>
        /// 购物车记录id
        /// </summary>
        public long CartRecordId { get; set; }
        /// <summary>
        /// 用户购物车中的记录id
        /// </summary>
        public string SkuId { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public int Quantity { get; set; }
        /// <summary>
        /// sku信息
        /// </summary>
        public SkuBasicInfo SkuBasicInfo { get; set; }
    }
}
