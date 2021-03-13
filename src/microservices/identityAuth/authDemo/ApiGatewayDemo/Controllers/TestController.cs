using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiGatewayDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly ILogger _logger;
        public TestController(ILogger<TestController> logger)
            => _logger = logger;

        [Authorize]
        [HttpGet("TestApiGatewayService")]
        public string TestApiGatewayService()
        {
            _logger.LogInformation("接收到请求，准备返回内容");
            return "测试api网关的接口能否把正常请求";
        }
    }
}
