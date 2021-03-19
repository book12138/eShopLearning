using eShopLearning.JdDataAnalysis.Dto;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopLearning.JdDataAnalysis.ApplicationServices.Impl
{
    public class SkuDataPublishToRabbitMQ : IDataPersistenceService
    {
        private readonly IConnectionFactory _rabbitmqConnFactory;
        private readonly ILogger _logger;
        public SkuDataPublishToRabbitMQ(IConnectionFactory rabbitmqConnFactory, ILogger<SkuDataPublishToRabbitMQ> logger)
        {
            _rabbitmqConnFactory = rabbitmqConnFactory;
            _logger = logger;
        }

        /// <summary>
        /// 将SKU 数据发送到RabbitMQ队列中
        /// </summary>
        /// <param name="jdSkuDtos"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        public async Task<bool> BatchSaveSkuData(IEnumerable<JdSkuDto> jdSkuDtos, string category)
        {
            if (!jdSkuDtos.Any())
                return true;

            using(var conn = _rabbitmqConnFactory.CreateConnection())
            {
                using (IModel channel = conn.CreateModel())
                {
                    channel.QueueDeclare(queue: "new_sku", durable: true, exclusive: false, autoDelete: false, arguments: null);
                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;

                    var body = JsonConvert.SerializeObject(new { Category = category, Skus = jdSkuDtos });
                    _logger.LogInformation("准备将抓取下来的SKU数据发送到RabbitMQ队列");
                    channel.BasicPublish(exchange: "",
                                 routingKey: "new_sku",
                                 mandatory: true,
                                 basicProperties: properties,
                                 body: Encoding.UTF8.GetBytes(body));
                    _logger.LogInformation("抓取到的SKU数据发送至RabbitMQ队列完毕");
                    return true;
                }
            }         
        }
    }
}
