using eShopLearning.Common;
using eShopLearning.WapAggregator.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.WapAggregator.ApplicationServices
{
    public interface ICartService
    {
        /// <summary>
        /// 获取用户购物车中所有商品
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<ResponseModel<IEnumerable<UserCartProductDto>>> GetUserCartAllProduct(long userId);
    }
}
