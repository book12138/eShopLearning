using ApiGatewayDemo.ApplicationServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ApiGatewayDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IApiTestService _apiTestService;
        public TestController(ILogger<TestController> logger, IApiTestService apiTestService)
        {
            _logger = logger;
            _apiTestService = apiTestService;
        }

        [Authorize]
        [HttpGet("TestApiGatewayService")]
        public string TestApiGatewayService()
        {
            _logger.LogInformation("接收到请求，准备返回内容");
            return "测试api网关的接口能否把正常请求，当你能接受到这条文字信息时，说明你已经成功请求api网关";
        }

        /// <summary>
        /// 测试请求 api demo
        /// </summary>
        /// <returns></returns>
        [HttpGet("TestRequestApiService")]
        [Authorize]
        public async Task<string> TestRequestApiService()
        {
            _logger.LogInformation("接收到请求，准备返回内容");
            return await _apiTestService.GetValue();
        }
    }
}
