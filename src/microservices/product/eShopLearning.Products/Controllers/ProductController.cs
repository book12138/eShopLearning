using eShopLearning.Products.ApplicationServices;
using eShopLearning.Products.ApplicationServices.Impl;
using eShopLearning.Products.Dto;
using eShopLearning.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eShopLearning.Products.Domain.Bus;
using eShopLearning.Products.Domain.Commands;
using MediatR;

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
        /// 总线
        /// </summary>
        private readonly IApplicationBus _applicationBus;
        /// <summary>
        /// 领域通知
        /// </summary>
        private readonly DomainNotificationHandler _domainNotification;

        /// <summary>
        /// 构造注入
        /// </summary>
        /// <param name="productService"></param>
        /// <param name="rabbitmqConnFactory"></param>
        /// <param name="skuEsService"></param>
        /// <param name="applicationBus"></param>
        /// <param name="notification"></param>
        public ProductController(
            IProductService productService, 
            IConnectionFactory rabbitmqConnFactory, 
            ISkuEsService skuEsService,
            IApplicationBus applicationBus,
            DomainNotificationHandler domainNotification
            )
        {
            _productService = productService;
            _rabbitmqConnFactory = rabbitmqConnFactory;
            _skuEsService = skuEsService;
            _applicationBus = applicationBus;
            _domainNotification = domainNotification;
        }

        /// <summary>
        /// 添加产品
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("AddProduct")]
        public async Task<ResponseModel> AddProduct([FromBody] AddProductDto dto)
        {
            //if (!dto.Skus.Any())
            //    return ResponseModel.BuildResponse(PublicStatusCode.Success);

            //using (var conn = _rabbitmqConnFactory.CreateConnection())
            //{
            //    using (IModel channel = conn.CreateModel())
            //    {
            //        channel.QueueDeclare(queue: "new_sku", durable: true, exclusive: false, autoDelete: false, arguments: null);
            //        var properties = channel.CreateBasicProperties();
            //        properties.Persistent = true;

            //        var body = JsonConvert.SerializeObject(dto);
            //        channel.BasicPublish(exchange: "",
            //                     routingKey: "new_sku",
            //                     mandatory: true,
            //                     basicProperties: properties,
            //                     body: Encoding.UTF8.GetBytes(body));
            //        return ResponseModel.BuildResponse(PublicStatusCode.Success);
            //    }
            //}

            // await _productService.AddProduct(dto);

            await _applicationBus.SendCommand(new AddProductCommand(dto.Category, dto.Skus));
            if (this._domainNotification.HasNotifications())
                return ResponseModel.BuildResponse(PublicStatusCode.Fail, 
                    string.Join(';', _domainNotification.GetNotifications().Select(u => u.Value)));

            return ResponseModel.BuildResponse(PublicStatusCode.Success);
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
