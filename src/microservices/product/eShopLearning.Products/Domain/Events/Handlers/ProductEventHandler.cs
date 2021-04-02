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
        /// 日志
        /// </summary>
        private readonly ILogger _logger;
        /// <summary>
        /// 构造注入
        /// </summary>
        /// <param name="skuEsService"></param>
        /// <param name="logger"></param>
        public ProductEventHandler(ILogger<ProductEventHandler> logger)
        {
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
            _logger.LogInformation("产品添加流程结束。注意此消息会重复一次以上，事件是多播，命令才是单播");
            await Task.CompletedTask;
        }
    }
}
