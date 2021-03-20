using AutoMapper;
using eShopLearning.Products.ApplicationServices;
using eShopLearning.Products.gRPC.Protos;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.Products.gRPC
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
        /// 构造注入
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="skuEsService"></param>
        /// <param name="productService"></param>
        /// <param name="mapper"></param>
        public SkuInfoGrpcService(
            ILogger<SkuInfoGrpcService> logger,
            ISkuEsService skuEsService,
            IProductService productService,
            IMapper mapper
            )
        {
            _logger = logger;
            _skuEsService = skuEsService;
            _productService = productService;
            _mapper = mapper;
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
            _logger.LogInformation("最终的查询结果为：{searchResult}", searchResult);
            if (searchResult is null || searchResult.Any() is false)
                return;
            
            foreach (var item in searchResult)
                await responseStream.WriteAsync(_mapper.Map<SearchReply>(item));
        }
    }
}
