using eShopLearning.Common;
using eShopLearning.Products.Dto;
using eShopLearning.Products.EFCoreRepositories.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace eShopLearning.Products.ApplicationServices
{
    public interface ISkuEsService
    {
        /// <summary>
        /// 将sku数据保存至es
        /// </summary>
        /// <param name="skus"></param>
        /// <returns></returns>
        Task SaveSkuData(IEnumerable<Sku> skus);
        /// <summary>
        /// 将sku数据保存至es
        /// </summary>
        /// <param name="sku"></param>
        /// <returns></returns>
        Task SaveSkuData(Sku sku);
        /// <summary>
        /// 搜索 SKU 
        /// </summary>
        /// <param name="keyword">关键字</param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        Task<IEnumerable<EsSkuDto>> Search(string keyword, int page, int size);
        /// <summary>
        /// 将数据库中的所有sku 数据存储到es中
        /// </summary>
        /// <returns></returns>
        Task SaveAllSkuDataToEsFromDb();
    }
}
