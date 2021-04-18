using eShopLearning.CartProductAggregator.ApplicationServices;
using eShopLearning.CartProductAggregator.Dto;
using eShopLearning.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace eShopLearning.CartProductAggregator.Controllers
{
    /// <summary>
    /// 购物车商品服务
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CartProductController : ControllerBase
    {
        private readonly ICartProductService _cartProductService;
        public CartProductController(ICartProductService cartProductService)
            => _cartProductService = cartProductService;

        /// <summary>
        /// 获取用户购物车中所有的商品
        /// </summary>
        /// <returns></returns>
#if DEBUG
        [HttpGet("GetUserCartProducts/{userId}")]
#endif
        [Authorize]
        public async Task<ResponseModel<IEnumerable<UserCartProductDto>>> GetUserCartProducts(long userId)
            => await _cartProductService.GetUserCartProduct(userId);

        /// <summary>
        /// 获取用户购物车中的商品
        /// </summary>
        /// <returns></returns>
#if DEBUG
        [HttpGet("GetUserCartProducts/{userId}/{page}/{size}")]
#endif
        [Authorize]
        public async Task<ResponseModel<IEnumerable<UserCartProductDto>>> GetUserCartProducts(long userId, int page, int size)
            => await _cartProductService.GetUserCartProduct(userId, page, size);
    }
}
