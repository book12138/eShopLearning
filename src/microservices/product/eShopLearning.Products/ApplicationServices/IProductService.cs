using eShopLearning.Products.Dto;
using eShopLearning.Products.EFCoreRepositories.Entities;
using eShopLearning.Products.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.Products.ApplicationServices
{
    public interface IProductService
    {
        /// <summary>
        /// 添加商品
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        Task<ResponseModel> AddProduct(AddProductDto dto);
    }
}
