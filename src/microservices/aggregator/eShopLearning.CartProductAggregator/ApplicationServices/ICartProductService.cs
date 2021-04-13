using eShopLearning.CartProductAggregator.Dto;
using eShopLearning.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.CartProductAggregator.ApplicationServices
{
    public interface ICartProductService
    {
        /// <summary>
        /// 获取用户购物车中所有商品
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        IAsyncEnumerable<ResponseModel<UserCartProductDto>> GetUserCartAllProduct(long userId);
        /// <summary>
        /// 获取用户购物车中的商品
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        IAsyncEnumerable<ResponseModel<UserCartProductDto>> GetUserCartProduct(long userId, int page, int size);
    }
}
