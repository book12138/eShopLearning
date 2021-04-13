using eShopLearning.Carts.ApplicationServices;
using eShopLearning.Carts.Dto;
using eShopLearning.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.Carts.Controllers
{
    /// <summary>
    /// 购物车服务
    /// </summary>
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class CartController : ControllerBase
    {
        /// <summary>
        /// 购物车应用服务
        /// </summary>
        private readonly ICartService _cartService;

        /// <summary>
        /// 构造注入
        /// </summary>
        /// <param name="cartService"></param>
        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        /// <summary>
        /// 添加至购物车
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("AddProductToCart/{userId}")]
        public async Task<ResponseModel> AddProductToCart(long userId, [FromBody] AddProductToCartDto req)
            => await _cartService.AddProductToCart(req.SkuId, userId, req.Quantity);

        /// <summary>
        /// 修改购物车中的商品数量
        /// </summary>
        /// <param name="cartRecordId"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        [HttpPut("UpdateCartProductQuantity/{cartRecordId}/{quantity}")]
        public async Task<ResponseModel> UpdateCartProductQuantity(long cartRecordId, int quantity)
            => await _cartService.UpdateCartProductQuantity(cartRecordId, quantity);

        /// <summary>
        /// 删除购物车中的一件商品
        /// </summary>
        /// <param name="productId"></param>        
        /// <returns></returns>
        [HttpDelete("RemoveCartProduct/{cartRecordId}")]
        public async Task<ResponseModel> RemoveCartProduct(long cartRecordId)
            => await _cartService.RemoveCartProduct(cartRecordId);

        /// <summary>
        /// 批量删除购物车中的商品
        /// </summary>
        /// <param name="cartRecordIds"></param>
        /// <returns></returns>
        [HttpPost("BatchRemoveCartProduct")]
        public async Task<ResponseModel> BatchRemoveCartProduct([FromBody] long[] cartRecordIds)
            => await _cartService.BatchRemoveCartProduct(cartRecordIds);

        /// <summary>
        /// 清空购物车
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpDelete("RemoveAllProduct/{userId}")]
        public async Task<ResponseModel> RemoveAllProduct(long userId)
            => await _cartService.RemoveAllProduct(userId);
    }
}
