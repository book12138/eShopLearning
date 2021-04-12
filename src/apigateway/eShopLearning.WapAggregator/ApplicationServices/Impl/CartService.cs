using AutoMapper;
using Consul;
using eShopLearning.Common;
using eShopLearning.WapAggregator.Dto;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace eShopLearning.WapAggregator.ApplicationServices.Impl
{
    public class CartService : ICartService
    {
        /// <summary>
        /// httpclient
        /// </summary>
        private readonly HttpClient _httpClient;
        /// <summary>
        /// 产品服务
        /// </summary>
        private readonly IProductService _productService;
        /// <summary>
        /// 日志
        /// </summary>
        private readonly ILogger _logger;
        /// <summary>
        /// 系统配置读取
        /// </summary>
        private readonly IConfiguration _configuration;
        /// <summary>
        /// automapper
        /// </summary>
        private readonly IMapper _mapper;

        /// <summary>
        /// 构造注入
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="productService"></param>
        /// <param name="logger"></param>
        /// <param name="configuration"></param>
        /// <param name="mapper"></param>
        public CartService(
            HttpClient httpClient, 
            IProductService productService,
            ILogger<CartService> logger,
            IConfiguration configuration,
            IMapper mapper
            )
        {
            _httpClient = httpClient;
            _productService = productService;
            _logger = logger;
            _configuration = configuration;
            _mapper = mapper;
        }

        /// <summary>
        /// 获取用户购物车中所有商品
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<ResponseModel<IEnumerable<UserCartProductDto>>> GetUserCartAllProduct(long userId)
        {
            using (var consulClient = new ConsulClient(a => a.Address = new Uri(_configuration["ConsulAddress"])))
            {
                var services = consulClient.Catalog.Service("microservice_carts").Result.Response;
                if (services != null && services.Any())
                {
                    var service = services.ElementAt(new Random().Next(services.Count()));

                    var response = await _httpClient.GetAsync($"http://{service.ServiceAddress}:{service.ServicePort}/api/Cart/GetUserCartAllProduct/{userId}");

                    if(response.StatusCode != System.Net.HttpStatusCode.OK) // 发送请求失败
                    {
                        _logger.LogError("请求购物车服务，查询用户购物车内容时请求不成功");
                        return ResponseModel<IEnumerable<UserCartProductDto>>.BuildResponse(PublicStatusCode.Fail, "查询购物车失败，请稍后重试");
                    }

                    var responseContent = await response.Content.ReadAsStringAsync();
                    if(responseContent is null or "")
                    {
                        _logger.LogError("请求购物车服务，查询用户购物车内容时，得到的结果是空");
                        return ResponseModel<IEnumerable<UserCartProductDto>>.BuildResponse(PublicStatusCode.Fail, "查询购物车失败，请稍后重试");
                    }

                    var temp = JsonConvert.DeserializeObject<ResponseModel<IEnumerable<UserCartQueryResponseDto>>>(responseContent);
                    if(temp.Code != (int)PublicStatusCode.Success) // 购物车服务内部检测到错误
                    {
                        _logger.LogError("请求购物车服务，查询用户购物车内容时请求不成功");
                        return ResponseModel<IEnumerable<UserCartProductDto>>.BuildResponse(PublicStatusCode.Fail, "查询购物车失败，请稍后重试");
                    }

                    var userCartProducts = _mapper.Map<IEnumerable<UserCartProductDto>>(temp.Data);
                    if(userCartProducts is not null && userCartProducts.Count() is not 0)
                        foreach (var item in userCartProducts)
                            item.SkuBasicInfo = await _productService.GetSkuBasikInfoAsId(item.SkuId);

                    return ResponseModel<IEnumerable<UserCartProductDto>>.BuildResponse(PublicStatusCode.Success, userCartProducts);
                }

                return ResponseModel<IEnumerable<UserCartProductDto>>.BuildResponse(PublicStatusCode.Fail, "查询购物车失败，请稍后重试");
            }
        }
    }
}
