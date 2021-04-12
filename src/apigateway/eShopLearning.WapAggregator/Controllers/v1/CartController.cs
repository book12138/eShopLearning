using eShopLearning.Common;
using eShopLearning.WapAggregator.ApplicationServices;
using eShopLearning.WapAggregator.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace eShopLearning.WapAggregator.Controllers.v1
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        public CartController(ICartService cartService) => _cartService = cartService;

        /// <summary>
        /// 获取用户购物车中的所有商品
        /// </summary>
        /// <returns></returns>
#if DEBUG
        [HttpGet("GetUserCartAllProduct/{userId}")]
#endif
        public async Task<ResponseModel<IEnumerable<UserCartProductDto>>> GetUserCartAllProduct(long userId)
            => await _cartService.GetUserCartAllProduct(userId);
    }
}