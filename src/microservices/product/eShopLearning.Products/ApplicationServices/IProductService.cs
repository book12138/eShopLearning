using eShopLearning.Products.Dto;
using eShopLearning.Products.EFCoreRepositories.Entities;
using eShopLearning.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eShopLearning.Products.ViewModel;

namespace eShopLearning.Products.ApplicationServices
{
    public interface IProductService
    {
        /// <summary>
        /// 添加商品
        /// </summary>
        /// <param name="category"></param>
        /// <param name="skuDtos"></param>
        /// <returns></returns>
        Task<(bool isSuccess, string errorMag, IEnumerable<Sku> skus)> AddProduct(string category, IEnumerable<SkuDto> skuDtos);
        /// <summary>
        /// 获取SKU详情
        /// </summary>
        /// <param name="skuId"></param>
        /// <returns></returns>
        Task<ResponseModel<ProductDetailsViewModel>> GetProductDetail(string skuId);
    }
}
