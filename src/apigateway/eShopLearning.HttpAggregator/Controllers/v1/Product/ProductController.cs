using eShopLearning.Common;
using eShopLearning.HttpAggregator.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.HttpAggregator.Controllers.v1.Product
{
    [Route("v1/product")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        /// <summary>
        /// 搜索
        /// </summary>
        /// <param name="keyword">关键字词</param>
        /// <param name="page">页数</param>
        /// <param name="size">显示数量</param>
        /// <returns></returns>
        [HttpGet("search/{keyword}/{page}/{size}")]
        public async ResponseModel<IEnumerable<SearchViewModel>> Search(string keyword, int page, int size)
        {
            return null;
        }
    }
}
