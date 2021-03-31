using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.HttpAggregator.Controllers.v1.Cart
{
    [Route("v1/cart")]
    [ApiController]
    public class CartController : ControllerBase
    {
        [HttpGet("get")]
        [Authorize]
        public IActionResult Get() => Content("哈哈哈哈哈哈哈哈，你通过了 token 检查");
    }
}