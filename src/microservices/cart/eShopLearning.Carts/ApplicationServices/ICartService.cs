using eShopLearning.Carts.Dto;
using eShopLearning.Common;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.Carts.ApplicationServices
{
    public interface ICartService
    {
        /// <summary>
        /// 添加至购物车
        /// </summary>
        /// <param name="skuId"></param>
        /// <param name="userId"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        Task<ResponseModel> AddProductToCart(string skuId, long userId, int quantity);
        /// <summary>
        /// 修改购物车中的商品数量
        /// </summary>
        /// <param name="cartRecordId"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        Task<ResponseModel> UpdateCartProductQuantity(long cartRecordId, int quantity);
        /// <summary>
        /// 删除购物车中的一件商品
        /// </summary>
        /// <param name="cartRecordId"></param>
        /// <returns></returns>
        Task<ResponseModel> RemoveCartProduct(long cartRecordId);
        /// <summary>
        /// 批量删除购物车中的商品
        /// </summary>
        /// <param name="cartRecordIds"></param>
        /// <returns></returns>
        Task<ResponseModel> BatchRemoveCartProduct(long[] cartRecordIds);
        /// <summary>
        /// 清空购物车
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<ResponseModel> RemoveAllProduct(long userId);
        /// <summary>
        /// 获取用户购物车中所有商品
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<ResponseModel<IEnumerable<UserCartProductDto>>> GetUserCartAllProduct(long userId);
    }
}
