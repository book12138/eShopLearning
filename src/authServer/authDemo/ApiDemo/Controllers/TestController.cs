using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [Authorize]
        [HttpGet("GetValue")]
        public string GetValue()
            => "今天天气很好 " + DateTime.Now + ",收到这条文字信息时，说明已成功请求通api";
    }
}
