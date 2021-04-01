using AutoMapper;
using eShopLearning.Carts.Dto;
using eShopLearning.Carts.EFCoreRepositories.EFCore;
using eShopLearning.Carts.EFCoreRepositories.Entities;
using eShopLearning.Carts.EFCoreRepositories.Repositories;
using eShopLearning.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.Carts.ApplicationServices.Impl
{
    public class CartService : ICartService
    {
        /// <summary>
        /// 购物车仓储
        /// </summary>
        private readonly ICartRepository _cartRepository;
        /// <summary>
        /// EF Core 上下文
        /// </summary>
        private readonly eShopCartDbContext _eShopCartDbContext;
        /// <summary>
        /// auto mapper
        /// </summary>
        private readonly IMapper _mapper;

        /// <summary>
        /// 构造注入
        /// </summary>
        /// <param name="cartRepository"></param>
        /// <param name="eShopCartDbContext"></param>
        /// <param name="mapper"></param>
        public CartService(
            ICartRepository cartRepository,
            eShopCartDbContext @eShopCartDbContext,
            IMapper mapper
            )
        {
            _cartRepository = cartRepository;
            _eShopCartDbContext = eShopCartDbContext;
            _mapper = mapper;
        }

        /// <summary>
        /// 添加至购物车
        /// </summary>
        /// <param name="skuId"></param>
        /// <param name="userId"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> AddProductToCart(string skuId, long userId, int quantity)
        {
            var oldCartRecord = await _eShopCartDbContext.Carts.Where(u => u.SkuId == skuId && u.UserId == userId).FirstOrDefaultAsync();
            if (oldCartRecord == null)
                await _cartRepository.AddAsync(new Cart() { SkuId = skuId, UserId = userId, Quantity = quantity });
            else
                oldCartRecord.Quantity += quantity;
            await _eShopCartDbContext.SaveChangesAsync();
            return ResponseModel.BuildResponse(PublicStatusCode.Success);
        }

        /// <summary>
        /// 修改购物车中的商品数量
        /// </summary>
        /// <param name="cartRecordId"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UpdateCartProductQuantity(long cartRecordId, int quantity)
        {
            var cartProductRecord = await _eShopCartDbContext.Carts.FirstOrDefaultAsync(u => u.Id == cartRecordId);
            if (cartProductRecord == null)
                return new ResponseModel { Code = 1001, Msg = "购物车中没有这件商品" };
            cartProductRecord.Quantity = quantity;
            await _eShopCartDbContext.SaveChangesAsync();
            return ResponseModel.BuildResponse(PublicStatusCode.Success);
        }

        /// <summary>
        /// 删除购物车中的一件商品
        /// </summary>
        /// <param name="cartRecordId"></param>
        /// <returns></returns>
        public async Task<ResponseModel> RemoveCartProduct(long cartRecordId)
        {
            await _cartRepository.RemoveAsync(cartRecordId);
            await _eShopCartDbContext.SaveChangesAsync();
            return ResponseModel.BuildResponse(PublicStatusCode.Success);
        }

        /// <summary>
        /// 批量删除购物车中的商品
        /// </summary>
        /// <param name="cartRecordIds"></param>
        /// <returns></returns>
        public async Task<ResponseModel> BatchRemoveCartProduct(long[] cartRecordIds)
        {
            await _cartRepository.BatchRemoveAsync(cartRecordIds);
            await _eShopCartDbContext.SaveChangesAsync();
            return ResponseModel.BuildResponse(PublicStatusCode.Success);
        }

        /// <summary>
        /// 清空购物车
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<ResponseModel> RemoveAllProduct(long userId)
        {
            await _cartRepository.BatchRemoveAsync(await _eShopCartDbContext.Carts.Where(u => u.UserId == userId).ToListAsync());
            await _eShopCartDbContext.SaveChangesAsync();
            return ResponseModel.BuildResponse(PublicStatusCode.Success);
        }

        /// <summary>
        /// 获取用户购物车中所有商品
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<ResponseModel<IEnumerable<UserCartProductDto>>> GetUserCartAllProduct(long userId)
            => new ResponseModel<IEnumerable<UserCartProductDto>> { Code = 200, Data = await _eShopCartDbContext.Carts.Where(u => u.UserId == userId).Select(u => new UserCartProductDto { SkuId = u.SkuId, Quantity = u.Quantity }).ToListAsync() };
    }
}
