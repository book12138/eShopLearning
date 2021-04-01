using eShopLearning.Products.ApplicationServices;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace eShopLearning.Products.Domain.Events.Handlers
{
    /// <summary>
    /// 产品相关的事件处理程序
    /// </summary>
    public class ProductEventHandler
        : INotificationHandler<AddProductEvent>
    {
        /// <summary>
        /// es 数据服务
        /// </summary>
        private readonly ISkuEsService _skuEsService;
        /// <summary>
        /// 日志
        /// </summary>
        private readonly ILogger _logger;
        /// <summary>
        /// 构造注入
        /// </summary>
        /// <param name="skuEsService"></param>
        /// <param name="logger"></param>
        public ProductEventHandler(ISkuEsService skuEsService, ILogger<ProductEventHandler> logger)
        {
            this._skuEsService = skuEsService;
            this._logger = logger;
        }

        /// <summary>
        /// 产品添加后，存一份产品数据到 es
        /// </summary>
        /// <param name="notification"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Handle(AddProductEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("将新产品数据存储到es中");
            await _skuEsService.SaveSkuData(notification.Skus);
            await Task.CompletedTask;
        }
    }
}
