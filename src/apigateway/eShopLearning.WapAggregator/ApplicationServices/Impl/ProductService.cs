using AutoMapper;
using Consul;
using eShopLearning.Common;
using eShopLearning.WapAggregator.ApplicationGrpcRemoteServices.Protos;
using eShopLearning.WapAggregator.Dto;
using eShopLearning.WapAggregator.ViewModel;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace eShopLearning.WapAggregator.ApplicationServices.Impl
{
    public class ProductService : IProductService
    {
        /// <summary>
        /// automapper
        /// </summary>
        private readonly IMapper _mapper;
        /// <summary>
        /// 系统配置读取
        /// </summary>
        private readonly IConfiguration _configuration;
        /// <summary>
        /// httpclient
        /// </summary>
        private readonly HttpClient _httpClient;

        /// <summary>
        /// 构造注入
        /// </summary>
        /// <param name="mapper"></param>
        /// <param name="configuration"></param>
        /// <param name="httpClient"></param>
        public ProductService(
            IMapper mapper, 
            IConfiguration configuration,
            HttpClient httpClient
            )
        {
            _mapper = mapper;
            _configuration = configuration;
            _httpClient = httpClient;
        }

        /// <summary>
        /// 搜索
        /// </summary>
        /// <param name="keyword">关键字</param>
        /// <param name="page">页码</param>
        /// <param name="size">每页显示的数量</param>
        /// <returns></returns>
        public async IAsyncEnumerable<ResponseModel<SearchViewModel>> Search(string keyword, int page, int size)
        {
            using (var consulClient = new ConsulClient(a => a.Address = new Uri(_configuration["ConsulAddress"])))
            {
                var services = consulClient.Catalog.Service("microservice_product_grpc").Result.Response;
                if (services != null && services.Any())
                {
                    var service = services.ElementAt(new Random().Next(services.Count()));

                    using var channel = GrpcChannel.ForAddress($"http://{service.ServiceAddress}:{service.ServicePort}/");
                    var client = new SkuInfoGrpc.SkuInfoGrpcClient(channel);
                    var reply = client.Search(new SearchRequest { Keyword = keyword, Page = page, Size = size });
                    await foreach (var item in reply.ResponseStream.ReadAllAsync())
                        yield return ResponseModel<SearchViewModel>.BuildResponse(PublicStatusCode.Success, _mapper.Map<SearchViewModel>(item));
                }
                else yield return ResponseModel<SearchViewModel>.BuildResponse(PublicStatusCode.Fail, "服务器异常，请稍后再试");
            }
        }

        /// <summary>
        /// 根据skuid获取sku基础信息
        /// </summary>
        /// <param name="skuId"></param>
        /// <returns></returns>
        public async Task<SkuBasicInfo> GetSkuBasikInfoAsId(string skuId)
        {
            using (var consulClient = new ConsulClient(a => a.Address = new Uri(_configuration["ConsulAddress"])))
            {
                var services = consulClient.Catalog.Service("microservice_product_grpc").Result.Response;
                if (services != null && services.Any())
                {
                    var service = services.ElementAt(new Random().Next(services.Count()));

                    using var channel = GrpcChannel.ForAddress($"http://{service.ServiceAddress}:{service.ServicePort}/");
                    var client = new SkuInfoGrpc.SkuInfoGrpcClient(channel);
                    
                    return _mapper.Map<SkuBasicInfo>(await client.GetSkuBasikInfoAsIdAsync(new GetSkuBasikInfoAsIdRequest { SkuId = skuId }));
                }

                return null;
            }
        }
    }
}
