using AutoFixture;
using AutoMapper;
using eShopLearning.Products.ApplicationServices;
using eShopLearning.Products.ApplicationServices.Impl;
using eShopLearning.Products.AutoMapper;
using eShopLearning.Products.Dto;
using eShopLearning.Products.EFCoreRepositories.EFCore;
using eShopLearning.Products.EFCoreRepositories.Entities;
using eShopLearning.Products.Infrastructure.Helper;
using eShopLearning.Users.EFCoreRepositories.Repositories;
using eShopLearning.Users.EFCoreRepositories.Repositories.Impl;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly IProductService _productService;
        private readonly eShopProductDbContext _eShopProductDbContext;

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

            var elasticsearchServiceMock = new Mock<ISkuEsService>();

            var serviceProvider = services.BuildServiceProvider();
            var _skuRepository = serviceProvider.GetService<ISkuRepository>();
            var _spuRepository = serviceProvider.GetService<ISpuRepository>();
            var _skuAttrRepository = serviceProvider.GetService<ISkuAttrRepository>();
            var _mapper = serviceProvider.GetService<IMapper>();
            _eShopProductDbContext = serviceProvider.GetService<eShopProductDbContext>();
            _productService = new ProductService(_skuRepository, _spuRepository, _skuAttrRepository, _mapper, _eShopProductDbContext, elasticsearchServiceMock.Object);
        }

        [Fact]
        public async Task AddProduct()
        {
            Fixture specimens = new Fixture();
            var dto = new AddProductDto { Category = "123", Skus = new List<SkuDto> {
                specimens.Create<SkuDto>(),
                specimens.Create<SkuDto>(),
                specimens.Create<SkuDto>()
            } };
            var result = await _productService.AddProduct(dto);
            result.Code.Should().Be(200);
            _eShopProductDbContext.Skus.Count().Should().Be(3);
        }
    }
}
