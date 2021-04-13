using eShopLearning.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.CartProductAggregator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartProductController : ControllerBase
    {
        /// <summary>
        /// 获取用户购物车中的商品
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetUserCartProducts")]
        [Authorize]
        public async Task<ResponseModel<IEnumerable<string>>> GetUserCartProducts() 
            => ResponseModel<IEnumerable<string>>.BuildResponse(PublicStatusCode.Success, "成功");
    }
}
