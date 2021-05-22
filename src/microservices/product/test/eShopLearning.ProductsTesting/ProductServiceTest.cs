using AutoFixture;
using AutoMapper;
using eShopLearning.Products.ApplicationServices;
using eShopLearning.Products.ApplicationServices.Impl;
using eShopLearning.Products.ApplicationServices.QueryServices;
using eShopLearning.Products.ApplicationServices.QueryServices.Impl;
using eShopLearning.Products.AutoMapper;
using eShopLearning.Products.Dto;
using eShopLearning.Products.EFCoreRepositories.EFCore;
using eShopLearning.Users.EFCoreRepositories.Repositories;
using eShopLearning.Users.EFCoreRepositories.Repositories.Impl;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace eShopLearning.ProductsTesting
{
    public class ProductServiceTest
    {
        private readonly ServiceProvider serviceProvider;

        public ProductServiceTest()
        {
            var services = new ServiceCollection();

            services.AddEntityFrameworkMySql() // ef core in memory
             .AddDbContext<eShopProductDbContext>(options => options.UseInMemoryDatabase("test"));
            services.AddAutoMapper(typeof(CustomProfile)); // automapper
            services.AddTransient<ISkuRepository, SkuRepository>();
            services.AddTransient<ISpuRepository, SpuRepository>();
            services.AddTransient<ISkuAttrRepository, SkuAttrRepository>();
            services.AddTransient<IProductService, ProductService>();
            services.AddTransient<IProductQueryService, ProductQueryService>();

            serviceProvider = services.BuildServiceProvider();            
        }

        /// <summary>
        /// 测试商品添加
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddProduct()
        {
            #region 创建 productservice 对象
            var skuRepository = serviceProvider.GetService<ISkuRepository>();
            var spuRepository = serviceProvider.GetService<ISpuRepository>();
            var skuAttrRepository = serviceProvider.GetService<ISkuAttrRepository>();
            var mapper = serviceProvider.GetService<IMapper>();
            var eShopProductDbContext = serviceProvider.GetService<eShopProductDbContext>();

            var mockLogger = new Mock<ILogger<ProductService>>();
            mockLogger.Setup(ml => ml.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()));
            var logger = mockLogger.Object;

            var mockProductQueryService = new Mock<IProductQueryService>();
            mockProductQueryService.Setup(u => u.ExistSku("test")).Returns(Task.Run(() => false));
            var productQueryService = mockProductQueryService.Object;

            var _productService = new ProductService(skuRepository, spuRepository, skuAttrRepository, mapper, eShopProductDbContext, logger, productQueryService);
            #endregion

            Fixture specimens = new Fixture();
            var dtos = new List<SkuDto> {
                specimens.Create<SkuDto>(),
                specimens.Create<SkuDto>(),
                specimens.Create<SkuDto>()
            };
            dtos = dtos.Select(u => { u.Title = "test"; return u; }).ToList();

            var result = await _productService.AddProduct("123", dtos);            
            result.isSuccess.Should().BeTrue();
            eShopProductDbContext.Skus.Count().Should().Be(3);
        }
    }
}
