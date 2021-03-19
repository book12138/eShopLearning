using eShopLearning.Products.Dto;
using eShopLearning.Products.EFCoreRepositories.Entities;
using eShopLearning.Products.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
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
        /// 搜索 SKU 
        /// </summary>
        /// <param name="keyword">关键字</param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        Task<IEnumerable<EsSkuDto>> Search(string keyword, int page, int size);
        Task SaveAllSkuDataToEsFromDb();
    }
}
