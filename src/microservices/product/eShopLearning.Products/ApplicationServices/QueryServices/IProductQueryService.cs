using eShopLearning.Products.EFCoreRepositories.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.Products.ApplicationServices.QueryServices
{
    public interface IProductQueryService
    {
        /// <summary>
        /// 根据sku id查询sku记录
        /// </summary>
        /// <param name="skuId">sku id</param>
        /// <param name="onlyUndeletedRecord">指定是否只查询未被删除的记录</param>
        /// <returns></returns>
        Task<Sku> QuerySkuAsId(string skuId, bool onlyUndeletedRecord = true);
        /// <summary>
        /// 根据 sku id 查询该sku的属性
        /// </summary>
        /// <param name="skuId">sku id</param>
        /// <param name="onlyUndeletedRecord">指定是否只查询未被删除的记录</param>
        /// <returns></returns>
        Task<IEnumerable<SkuAttr>> QuerySkuAttrsAsSkuId(string skuId, bool onlyUndeletedRecord = true);
        /// <summary>
        /// 根据 spu id 查询该spu下所有sku的属性
        /// </summary>
        /// <param name="skuId">sku id</param>
        /// <param name="onlyUndeletedRecord">指定是否只查询未被删除的记录</param>
        /// <returns></returns>
        Task<IEnumerable<SkuAttr>> QueryAllSkuAttrsAsSpuId(string spuId, bool onlyUndeletedRecord = true);
        /// <summary>
        /// 检查是否存在此商品
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        Task<bool> ExistSku(string title);
    }
}
