using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;

namespace ClientDemo.ApplicationServices.Impl
{
    public class TestService : ITestService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        
        public TestService(HttpClient httpClient, ILogger<TestService> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// 检查一下能否和api网关进行正常通信
        /// </summary>
        /// <returns></returns>
        public async Task<string> TestApiGatewayService()
        {
            _logger.LogInformation("准备开始请求");
            var response = await _httpClient.GetAsync(_configuration["ApiGatewayUrl"] + "/api/Test/TestApiGatewayService");
            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation($"请求完毕，响应码为 {(int)response.StatusCode}, 返回内容为：{responseContent}");
            return responseContent;
        }
    }
}
