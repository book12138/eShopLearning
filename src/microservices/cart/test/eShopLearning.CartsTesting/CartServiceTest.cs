using eShopLearning.Carts.ApplicationServices;
using eShopLearning.Carts.ApplicationServices.Impl;
using eShopLearning.Carts.AutoMapper;
using eShopLearning.Carts.EFCoreRepositories.EFCore;
using eShopLearning.Carts.EFCoreRepositories.Repositories;
using eShopLearning.Carts.EFCoreRepositories.Repositories.Impl;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace eShopLearning.CartsTesting
{
    public class CartServiceTest
    {
        private readonly ICartService _cartService;
        private readonly eShopCartDbContext _eShopCartDbContext;

        public CartServiceTest()
        {
            var services = new ServiceCollection();

            services.AddEntityFrameworkMySql() // ef core in memory
                .AddDbContext<eShopCartDbContext>(options => options.UseInMemoryDatabase("test"));
            services.AddAutoMapper(typeof(CustomProfile)); // automapper
            services.AddTransient<ICartRepository, CartRepository>();
            services.AddTransient<ICartService, CartService>();

            var serviceProvider = services.BuildServiceProvider();
            _cartService = serviceProvider.GetService<ICartService>();
            _eShopCartDbContext = serviceProvider.GetService<eShopCartDbContext>();

            #region ef core 上下文初始化一些数据
            _eShopCartDbContext.Carts.Add(new Carts.EFCoreRepositories.Entities.Cart { Id = 1, Quantity = 2, SkuId = "123456", UserId = 1 });
            _eShopCartDbContext.SaveChanges();
            #endregion
        }

        /// <summary>
        /// 添加一条新的不存在的购物车记录
        /// </summary>
        [Fact]
        public async Task AddNewCartRecord()
        {
            var result = await _cartService.AddProductToCart("456789", 2, 1);
            result.Should().NotBeNull();
            result.Code.Should().Be(200);
        }

        /// <summary>
        /// 添加一条已存在的购物车记录
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddsAnExistingRecord()
        {
            var result = await _cartService.AddProductToCart("123456", 1, 1);
            result.Should().NotBeNull();
            result.Code.Should().Be(200);
            var record = await _eShopCartDbContext.Carts.FirstOrDefaultAsync(u => u.Id == 1);
            record.Should().NotBeNull();
            record.Quantity.Should().Be(3);
        }

        /// <summary>
        /// 向一条已存在的购物车记录修改数量
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ModifyTheQuantityOfAnExistingRecord()
        {
            var result = await _cartService.UpdateCartProductQuantity(1, 4);
            result.Should().NotBeNull();
            result.Code.Should().Be(200);
            var record = await _eShopCartDbContext.Carts.FirstOrDefaultAsync(u => u.Id == 1);
            record.Should().NotBeNull();
            record.Quantity.Should().Be(4);
        }

        /// <summary>
        /// 向一条不存在的购物车记录修改数量
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ModifyTheQuantityOfAnNotExistingRecord()
        {
            var result = await _cartService.UpdateCartProductQuantity(2, 4);
            result.Should().NotBeNull();
            result.Code.Should().Be(1001);
        }

        /// <summary>
        /// 删除一条购物车记录
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task RemoveACartRecord()
        {
            {
                var result = await _cartService.RemoveCartProduct(1); // 删除一条已存在的购物车记录
                result.Should().NotBeNull();
                result.Code.Should().Be(200);
            }

            {
                var result = await _cartService.RemoveCartProduct(2); // 删除一条不存在的购物车记录
                result.Should().NotBeNull();
                result.Code.Should().Be(200);
            }
        }
        
        /// <summary>
        /// 获取用户所有购物车记录
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetUserAllCartRecord()
        {
            var result = await _cartService.GetUserCartAllProduct(1);
            result.Should().NotBeNull();
            result.Code.Should().Be(200);
            result.Data.Should().NotBeNull();
            result.Data.Count().Should().Be(1);
        }
    }
}
