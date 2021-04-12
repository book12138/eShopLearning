using AutoMapper;
using eShopLearning.Products.ApplicationServices;
using eShopLearning.Products.ApplicationGrpcRemoteServices.Protos;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eShopLearning.Products.EFCoreRepositories.EFCore;
using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore;

namespace eShopLearning.Products.ApplicationGrpcRemoteServices
{
    public class SkuInfoGrpcService : SkuInfoGrpc.SkuInfoGrpcBase
    {
        /// <summary>
        /// 日志
        /// </summary>
        private readonly ILogger _logger;
        /// <summary>
        /// sku es 服务
        /// </summary>
        private readonly ISkuEsService _skuEsService;
        /// <summary>
        /// 产品服务
        /// </summary>
        private readonly IProductService _productService;
        /// <summary>
        /// automapper
        /// </summary>
        private readonly IMapper _mapper;
        /// <summary>
        /// ef Core
        /// </summary>
        private readonly eShopProductDbContext _eShopProductDbContext;

        /// <summary>
        /// 构造注入
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="skuEsService"></param>
        /// <param name="productService"></param>
        /// <param name="mapper"></param>
        /// <param name="eShopProductDbContext"></param>
        public SkuInfoGrpcService(
            ILogger<SkuInfoGrpcService> logger,
            ISkuEsService skuEsService,
            IProductService productService,
            IMapper mapper,
            eShopProductDbContext eShopProductDbContext
            )
        {
            _logger = logger;
            _skuEsService = skuEsService;
            _productService = productService;
            _mapper = mapper;
            _eShopProductDbContext = eShopProductDbContext;
        }

        /// <summary>
        /// 产品搜索
        /// </summary>
        /// <param name="request"></param>
        /// <param name="responseStream"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task Search(SearchRequest request, IServerStreamWriter<SearchReply> responseStream, ServerCallContext context)
        {
            _logger.LogInformation("本次搜索的关键字词是：{keyword}", request.Keyword);
            var searchResult = await _skuEsService.Search(request.Keyword, request.Page, request.Size);
            _logger.LogInformation("最终的查询得到的数据条数为：{searchResultCount}", searchResult.Count());
            if (searchResult is null || searchResult.Any() is false)
                return;
            
            foreach (var item in searchResult)
                await responseStream.WriteAsync(_mapper.Map<SearchReply>(item));
        }

        /// <summary>
        /// 根据sku id获取sku的基础信息
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<GetSkuBasikInfoAsIdReply> GetSkuBasikInfoAsId(GetSkuBasikInfoAsIdRequest request, ServerCallContext context)
        {
            _logger.LogInformation("获取sku {id} 的数据", request.SkuId);
            if (request.SkuId is null or "")
                return null;

            var sku = _eShopProductDbContext.Skus.FirstOrDefault(u => u.Id == request.SkuId);
            if (sku == null)
            {
                _logger.LogWarning("该sku {id} 查找不到", request.SkuId);
                return null;
            }

            var result = _mapper.Map<GetSkuBasikInfoAsIdReply>(sku);
            result.SkuAttrs = string.Join(',', 
                await _eShopProductDbContext.SkuAttrs.Where(u => u.Status && u.SkuId == request.SkuId)?.Select(u => u.Name).ToListAsync() ?? new List<string>());

            return result;
        }
    }
}
