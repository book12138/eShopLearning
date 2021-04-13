using AutoMapper;
using Consul;
using eShopLearning.CartProductAggregator.ApplicationGrpcRemoteServices.Protos;
using eShopLearning.CartProductAggregator.Dto;
using eShopLearning.Common;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.CartProductAggregator.ApplicationServices.Impl
{
    public class CartProductService : ICartProductService
    {
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
        private IConfiguration _configuration;

        /// <summary>
        /// 构造注入
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="mapper"></param>
        /// <param name="configuration"></param>
        public CartProductService(
            ILogger<CartProductService> logger,
            IMapper mapper,
            IConfiguration configuration
            )
        {
            _logger = logger;
            _mapper = mapper;
            _configuration = configuration;
        }

        /// <summary>
        /// 获取用户购物车中所有商品
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<ResponseModel<UserCartProductDto>> GetUserCartAllProduct(long userId)
        {
            using (var consulClient = new ConsulClient(a => a.Address = new Uri(_configuration["ConsulAddress"])))
            {
                var cartServices = consulClient.Catalog.Service("microservice_carts_grpc").Result.Response;
                var productServices = consulClient.Catalog.Service("microservice_products_grpc").Result.Response;
                if (cartServices != null && cartServices.Any()
                    && productServices != null && productServices.Any())
                {
                    var cartCatalogService = cartServices.ElementAt(new Random().Next(cartServices.Count()));
                    var productCatalogService = productServices.ElementAt(new Random().Next(productServices.Count()));

                    var cartClient = new CartProductGrpc.CartProductGrpcClient(GrpcChannel.ForAddress($"http://{cartCatalogService.ServiceAddress}:{cartCatalogService.ServicePort}/"));
                    var productClient = new SkuInfoGrpc.SkuInfoGrpcClient(GrpcChannel.ForAddress($"http://{productCatalogService.ServiceAddress}:{productCatalogService.ServicePort}/"));

                    var reply = cartClient.GetUserCartAllProduct(new GetUserCartAllProductRequest { UserId = userId.ToString() });
                    await foreach (var item in reply.ResponseStream.ReadAllAsync())
                    {
                        var userCartProductDto = _mapper.Map<UserCartProductDto>(item);
                        if(userCartProductDto is null)
                            yield return ResponseModel<UserCartProductDto>.BuildResponse(PublicStatusCode.Fail, "服务器异常，一条购物车记录加载失败");

                        var temp = await productClient.GetSkuBasikInfoAsIdAsync(new GetSkuBasikInfoAsIdRequest() { SkuId = userCartProductDto?.SkuId });
                        userCartProductDto.SkuBasicInfo = _mapper.Map<SkuBasicInfo>(temp);

                        yield return ResponseModel<UserCartProductDto>.BuildResponse(PublicStatusCode.Success, userCartProductDto); ;
                    }
                }
                else yield return ResponseModel<UserCartProductDto>.BuildResponse(PublicStatusCode.Fail, "服务器异常，请稍后再试");
            }
        }

        /// <summary>
        /// 获取用户购物车中的商品
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<ResponseModel<UserCartProductDto>> GetUserCartProduct(long userId, int page, int size)
        {
            using (var consulClient = new ConsulClient(a => a.Address = new Uri(_configuration["ConsulAddress"])))
            {
                var cartServices = consulClient.Catalog.Service("microservice_carts_grpc").Result.Response;
                var productServices = consulClient.Catalog.Service("microservice_products_grpc").Result.Response;
                if (cartServices != null && cartServices.Any()
                    && productServices != null && productServices.Any())
                {
                    var cartCatalogService = cartServices.ElementAt(new Random().Next(cartServices.Count()));
                    var productCatalogService = productServices.ElementAt(new Random().Next(productServices.Count()));

                    var cartClient = new CartProductGrpc.CartProductGrpcClient(GrpcChannel.ForAddress($"http://{cartCatalogService.ServiceAddress}:{cartCatalogService.ServicePort}/"));
                    var productClient = new SkuInfoGrpc.SkuInfoGrpcClient(GrpcChannel.ForAddress($"http://{productCatalogService.ServiceAddress}:{productCatalogService.ServicePort}/"));

                    var reply = cartClient.GetUserCartProduct(new GetUserCartProductRequest { UserId = userId.ToString(), Page = page, Size = size });
                    await foreach (var item in reply.ResponseStream.ReadAllAsync())
                    {
                        var userCartProductDto = _mapper.Map<UserCartProductDto>(item);
                        if (userCartProductDto is null)
                            yield return ResponseModel<UserCartProductDto>.BuildResponse(PublicStatusCode.Fail, "服务器异常，一条购物车记录加载失败");

                        var temp = await productClient.GetSkuBasikInfoAsIdAsync(new GetSkuBasikInfoAsIdRequest() { SkuId = userCartProductDto?.SkuId });
                        userCartProductDto.SkuBasicInfo = _mapper.Map<SkuBasicInfo>(temp);

                        yield return ResponseModel<UserCartProductDto>.BuildResponse(PublicStatusCode.Success, userCartProductDto); ;
                    }
                }
                else yield return ResponseModel<UserCartProductDto>.BuildResponse(PublicStatusCode.Fail, "服务器异常，请稍后再试");
            }
        }
    }
}
