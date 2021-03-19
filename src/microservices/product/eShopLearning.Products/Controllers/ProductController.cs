using eShopLearning.Products.ApplicationServices;
using eShopLearning.Products.ApplicationServices.Impl;
using eShopLearning.Products.Dto;
using eShopLearning.Products.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopLearning.Products.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        /// <summary>
        /// 商品服务
        /// </summary>
        private readonly IProductService _productService;
        /// <summary>
        /// rabbitmq队列
        /// </summary>
        private readonly IConnectionFactory _rabbitmqConnFactory;
        /// <summary>
        /// sku es 服务
        /// </summary>
        private readonly ISkuEsService _skuEsService;
        /// <summary>
        /// 注入
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// 构造注入
        /// </summary>
        /// <param name="productService"></param>
        /// <param name="rabbitmqConnFactory"></param>
        /// <param name="skuEsService"></param>
        /// <param name="logger"></param>
        public ProductController(
            IProductService productService, 
            IConnectionFactory rabbitmqConnFactory, 
            ISkuEsService skuEsService,
            ILogger<ProductService> logger
            )
        {
            _productService = productService;
            _rabbitmqConnFactory = rabbitmqConnFactory;
            _skuEsService = skuEsService;
            _logger = logger;
        }

        /// <summary>
        /// 添加产品
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("AddProduct")]
        public async Task<ResponseModel> AddProduct([FromBody] AddProductDto dto)
        {
            if (!dto.Skus.Any())
                return ResponseModel.BuildResponse(PublicStatusCode.Success);

            using (var conn = _rabbitmqConnFactory.CreateConnection())
            {
                using (IModel channel = conn.CreateModel())
                {
                    channel.QueueDeclare(queue: "new_sku", durable: true, exclusive: false, autoDelete: false, arguments: null);
                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;

                    var body = JsonConvert.SerializeObject(dto);
                    channel.BasicPublish(exchange: "",
                                 routingKey: "new_sku",
                                 mandatory: true,
                                 basicProperties: properties,
                                 body: Encoding.UTF8.GetBytes(body));
                    return ResponseModel.BuildResponse(PublicStatusCode.Success);
                }
            }

            // await _productService.AddProduct(dto);
        }

        /// <summary>
        /// 搜索
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet("Search/{keyword}/{page}/{size}")]
        public async Task<IEnumerable<EsSkuDto>> Search(string keyword, int page, int size)
            => await _skuEsService.Search(keyword, page, size);

        [HttpGet("AddAllSkuToEs")]
        public async Task<IActionResult> AddAllSkuToEs()
        {
            await _skuEsService.SaveAllSkuDataToEsFromDb();
            return Ok();
        }
    }
}
