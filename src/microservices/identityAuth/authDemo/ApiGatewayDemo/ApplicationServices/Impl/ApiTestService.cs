using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ApiGatewayDemo.ApplicationServices.Impl
{
    public class ApiTestService : IApiTestService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public ApiTestService(IConfiguration configuration, HttpClient httpClient, ILogger<ApiTestService> logger)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>
        /// 尝试请求 api service 的getvalue
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetValue()
        {
            var response = await _httpClient.GetAsync(_configuration["ApiDemoUrl"] + "/api/Test/GetValue");
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                _logger.LogError("并未成功请求通接口");
            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation(responseContent);
            return responseContent;
        }
    }
}
