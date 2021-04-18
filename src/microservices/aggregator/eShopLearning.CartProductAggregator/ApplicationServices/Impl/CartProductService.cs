using AutoMapper;
using Consul;
using eShopLearning.CartProductAggregator.ApplicationGrpcRemoteServices.Protos;
using eShopLearning.CartProductAggregator.Dto;
using eShopLearning.Common;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Threading.Tasks;

namespace eShopLearning.CartProductAggregator.ApplicationServices.Impl
{
    public class CartProductService : ICartProductService
    {
        #region 字段
        /// <summary>
        /// 日志
        /// </summary>
        private readonly ILogger _logger;
        /// <summary>
        /// automapper
        /// </summary>
        private readonly IMapper _mapper;
        /// <summary>
        /// 系统配置读取
        /// </summary>
        private readonly IConfiguration _configuration;
        /// <summary>
        /// consul
        /// </summary>
        private readonly IConsulClient _consulClient;
        /// <summary>
        /// 全局默认超时策略
        /// </summary>
        private readonly ITimeoutPolicy _defaultTimeoutPolicy;
        /// <summary>
        /// 全局默认熔断策略
        /// </summary>
        private readonly ICircuitBreakerPolicy _defaultCircuitBreakerPolicy;
        #endregion

        #region 属性
        /// <summary>
        /// 购物车gRPC服务节点信息数组
        /// </summary>
        protected CatalogService[] CartRrpcCatalogServices => _consulClient.Catalog.Service("microservice_carts_grpc").Result.Response;
        /// <summary>
        /// 商品gRPC服务节点信息数组
        /// </summary>
        protected CatalogService[] ProductRrpcCatalogServices => _consulClient.Catalog.Service("microservice_product_grpc").Result.Response;
        #endregion

        /// <summary>
        /// 构造注入
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="mapper"></param>
        /// <param name="configuration"></param>
        /// <param name="consulClient"></param>
        /// <param name="timeoutPolicy"></param>
        /// <param name="circuitBreakerPolicy"></param>
        public CartProductService(
            ILogger<CartProductService> logger,
            IMapper mapper,
            IConfiguration configuration,
            IConsulClient consulClient,
            ITimeoutPolicy timeoutPolicy,
            ICircuitBreakerPolicy circuitBreakerPolicy
            )
        {
            _logger = logger;
            _mapper = mapper;
            _configuration = configuration;
            _consulClient = consulClient;
            _defaultTimeoutPolicy = timeoutPolicy;
            _defaultCircuitBreakerPolicy = circuitBreakerPolicy;
        }

        /// <summary>
        /// 获取用户购物车中所有商品
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<ResponseModel<IEnumerable<UserCartProductDto>>> GetUserCartProduct(long userId)
        {
            var cartServices = CartRrpcCatalogServices;

            if (cartServices is null || cartServices.Any() is false)
                return ResponseModel<IEnumerable<UserCartProductDto>>.BuildResponse(PublicStatusCode.Fail, "服务器异常，请稍后再试");

            var result = new List<UserCartProductDto>();

            ISyncPolicy policy = Polly.Policy.Wrap(
                Polly.Policy.Timeout(20, TimeoutStrategy.Pessimistic), // 定义超时时间设定为 20s
                _defaultCircuitBreakerPolicy as ISyncPolicy);
            await policy.Execute(async () =>
            {
                var service = cartServices.ElementAt(new Random().Next(cartServices.Count()));
                var client = new CartProductGrpc.CartProductGrpcClient(GrpcChannel.ForAddress($"http://{service.ServiceAddress}:{service.ServicePort}/"));
                var reply = client.GetUserCartAllProduct(new GetUserCartAllProductRequest { UserId = userId.ToString() });
                await foreach (var item in reply.ResponseStream.ReadAllAsync())
                {
                    var userCartProductDto = _mapper.Map<UserCartProductDto>(item);
                    if (userCartProductDto is null)
                        continue;

                    userCartProductDto.SkuBasicInfo = await GetSkuBasicInfo(item.SkuId);
                    result.Add(userCartProductDto);
                }
            });
           
            return ResponseModel<IEnumerable<UserCartProductDto>>.BuildResponse(PublicStatusCode.Success, result);
        }

        /// <summary>
        /// 获取用户购物车中的商品
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public async Task<ResponseModel<IEnumerable<UserCartProductDto>>> GetUserCartProduct(long userId, int page, int size)
        {
            var cartServices = CartRrpcCatalogServices;

            if (cartServices is null || cartServices.Any() is false)
                return ResponseModel<IEnumerable<UserCartProductDto>>.BuildResponse(PublicStatusCode.Fail, "服务器异常，请稍后再试");

            var result = new List<UserCartProductDto>();

            ISyncPolicy policy = Polly.Policy.Wrap(
                Polly.Policy.Timeout(10, TimeoutStrategy.Pessimistic), // 定义超时时间设定为 10s
                _defaultCircuitBreakerPolicy as ISyncPolicy);
            await policy.Execute(async () =>
            {
                var service = cartServices.ElementAt(new Random().Next(cartServices.Count()));
                var client = new CartProductGrpc.CartProductGrpcClient(GrpcChannel.ForAddress($"http://{service.ServiceAddress}:{service.ServicePort}/"));
                var reply = client.GetUserCartProduct(new GetUserCartProductRequest { UserId = userId.ToString(), Page = page, Size = size });
                await foreach (var item in reply.ResponseStream.ReadAllAsync())
                {
                    var userCartProductDto = _mapper.Map<UserCartProductDto>(item);
                    if (userCartProductDto is null)
                        continue;

                    userCartProductDto.SkuBasicInfo = await GetSkuBasicInfo(item.SkuId);
                    result.Add(userCartProductDto);
                }
            });

            return ResponseModel<IEnumerable<UserCartProductDto>>.BuildResponse(PublicStatusCode.Success, result);
        }

        /// <summary>
        /// 获取SKU基础信息
        /// </summary>
        /// <param name="skuId"></param>
        /// <returns></returns>
        private async Task<SkuBasicInfo> GetSkuBasicInfo(string skuId)
        {
            var productServices = ProductRrpcCatalogServices;
            if (productServices is null || productServices.Any() is false || skuId is null or "")
                return null;

            SkuBasicInfo result = null;

            ISyncPolicy policy = Polly.Policy.Wrap(_defaultTimeoutPolicy as ISyncPolicy, _defaultCircuitBreakerPolicy as ISyncPolicy);
            await policy.Execute(async () =>
            {
                var service = productServices.ElementAt(new Random().Next(productServices.Count()));
                var client = new SkuInfoGrpc.SkuInfoGrpcClient(GrpcChannel.ForAddress($"http://{service.ServiceAddress}:{service.ServicePort}/"));

                var reply = await client.GetSkuBasikInfoAsIdAsync(new GetSkuBasikInfoAsIdRequest() { SkuId = skuId });
                if (reply is not null)
                    result = _mapper.Map<SkuBasicInfo>(reply);
            });

            return result;
        }
    }
}
