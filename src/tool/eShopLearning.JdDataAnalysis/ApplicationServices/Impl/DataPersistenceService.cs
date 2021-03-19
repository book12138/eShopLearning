using eShopLearning.JdDataAnalysis.Dto;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace eShopLearning.JdDataAnalysis.ApplicationServices.Impl
{
    public class DataPersistenceService : IDataPersistenceService
    {
        /// <summary>
        /// 日志
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// 构造注入
        /// </summary>
        /// <param name="logger"></param>
        public DataPersistenceService(ILogger<DataPersistenceService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 批量保存抓取到的 SKU 数据
        /// </summary>
        /// <param name="jdSkuDtos"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        public async Task<bool> BatchSaveSkuData(IEnumerable<JdSkuDto> jdSkuDtos, string category)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                var encodedContent = new StringContent(JsonConvert.SerializeObject(new { Category = category, Skus = jdSkuDtos }));
                encodedContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = await httpClient.PostAsync("http://localhost:7648/api/Product/AddProduct", encodedContent);
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    _logger.LogError("添加商品失败");
                    return false;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(responseContent))
                {
                    _logger.LogError("添加商品失败");
                    return false;
                }

                JObject jObj = JObject.Parse(responseContent);
                var code = jObj?["code"]?.Value<string>();
                if (string.IsNullOrEmpty(code))
                    return false;
                if (code != "200")
                {
                    _logger.LogError("保存SKU数据时失败，失败原因为：" + jObj?["msg"]?.Value<string>());
                    return false;
                }

                return true;
            }
        }
    }
}
