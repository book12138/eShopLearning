using eShopLearning.Common;
using eShopLearning.WapAggregator.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace eShopLearning.WapAggregator.ApplicationServices.Impl
{
    public class CartService : ICartService
    {
        /// <summary>
        /// httpclient
        /// </summary>
        private readonly HttpClient _httpClient;

        /// <summary>
        /// 构造注入
        /// </summary>
        /// <param name="httpClient"></param>
        public CartService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// 获取用户购物车中所有商品
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<ResponseModel<IEnumerable<UserCartProductDto>>> GetUserCartAllProduct(long userId)
        {

        }
    }
}
