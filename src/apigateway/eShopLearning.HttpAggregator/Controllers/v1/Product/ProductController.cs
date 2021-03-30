using AutoMapper;
using Consul;
using eShopLearning.Common;
using eShopLearning.HttpAggregator.ApplicationServices;
using eShopLearning.HttpAggregator.gRPC.Protos;
using eShopLearning.HttpAggregator.ViewModel;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.HttpAggregator.Controllers.v1.Product
{
    /// <summary>
    /// 产品服务
    /// </summary>
    [Route("v1/product")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly IProductService _productService;
        public ProductController(
            IConfiguration configuration, 
            IMapper mapper,
            IProductService productService
            )
        {
            _configuration = configuration;
            _mapper = mapper;
            _productService = productService;
        }

        /// <summary>
        /// 搜索
        /// </summary>
        /// <param name="keyword">关键字词</param>
        /// <param name="page">页数</param>
        /// <param name="size">显示数量</param>
        /// <returns></returns>
        [HttpGet("search/{keyword}/{page}/{size}")]
        public async IAsyncEnumerable<ResponseModel<SearchViewModel>> Search(string keyword, int page, int size)
        {
            await foreach (var item in _productService.Search(keyword, page, size)) 
                yield return item;
        }

        ///// <summary>
        ///// 搜索
        ///// </summary>
        ///// <param name="keyword">关键字词</param>
        ///// <param name="page">页数</param>
        ///// <param name="size">显示数量</param>
        ///// <returns></returns>
        //[HttpGet("search/{keyword}/{page}/{size}")]
        //public async Task<ResponseModel<IEnumerable<SearchViewModel>>> Search(string keyword, int page, int size)
        //{
        //    List<SearchViewModel> searchResult = null;
        //    using (var consulClient = new ConsulClient(a => a.Address = new Uri(_configuration["ConsulAddress"])))
        //    {
        //        var services = consulClient.Catalog.Service("microservice_product_grpc").Result.Response;
        //        if (services != null && services.Any())
        //        {
        //            // 模拟随机一台进行请求，这里只是测试，可以选择合适的负载均衡框架
        //            var service = services.ElementAt(new Random().Next(services.Count()));

        //            using var channel = GrpcChannel.ForAddress($"http://{service.ServiceAddress}:{service.ServicePort}/Search");
        //            var client = new SkuInfoGrpc.SkuInfoGrpcClient(channel);
        //            var reply = client.Search(new SearchRequest { Keyword = keyword, Page = page, Size = size });
        //            searchResult = new List<SearchViewModel>();
        //            await foreach (var item in reply.ResponseStream.ReadAllAsync())
        //                searchResult.Add(_mapper.Map<SearchViewModel>(item));                    
        //        }
        //    }
        //    return new ResponseModel<IEnumerable<SearchViewModel>> { Code = 200, Data = searchResult };
        //}
    }
}
