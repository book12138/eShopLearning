using AutoMapper;
using eShopLearning.Common;
using eShopLearning.WapAggregator.ApplicationServices;
using eShopLearning.WapAggregator.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace eShopLearning.WapAggregator.Controllers.v1
{
    /// <summary>
    /// 产品服务
    /// </summary>
    [Route("api/v1/[controller]")]
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
    }
}
