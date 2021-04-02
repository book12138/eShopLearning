using eShopLearning.Products.ApplicationServices;
using eShopLearning.Products.Domain.Base;
using eShopLearning.Products.Domain.Bus;
using eShopLearning.Products.Domain.Events;
using eShopLearning.Products.EFCoreRepositories.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace eShopLearning.Products.Domain.Commands.Handlers
{
    /// <summary>
    /// 产品相关的命令处理逻辑
    /// </summary>
    public class ProductCommandHandler : DomainCommandHandler, 
        IRequestHandler<AddProductCommand>,
        IRequestHandler<SkuInfoPersistentToEsCommand>
    {
        /// <summary>
        /// 产品应用服务
        /// </summary>
        private readonly IProductService _productService;
        /// <summary>
        /// 日志
        /// </summary>
        private readonly ILogger _logger;
        /// <summary>
        /// sku es
        /// </summary>
        private readonly ISkuEsService _skuEsService;

        /// <summary>
        /// 构造注入
        /// </summary>
        /// <param name="mediatorHandler"></param>
        /// <param name="productService"></param>
        /// <param name="logger"></param>
        /// <param name="skuEsService"></param>
        public ProductCommandHandler(
            IApplicationBus mediatorHandler,
            IProductService productService,
            ILogger<ProductCommandHandler> logger,
            ISkuEsService skuEsService
            )
            :base(mediatorHandler)
        {
            this._productService = productService;
            this._logger = logger;
            _skuEsService = skuEsService;
        }

        /// <summary>
        /// 产品添加命令处理程序
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Unit> Handle(AddProductCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("准备开始添加新产品至数据库");
            (bool isSuccess, string errorMsg, IEnumerable<Sku> skus) = await this._productService.AddProduct(request.Category, request.SkuDtos);
            _logger.LogInformation("添加结束，结果为：{result}", isSuccess ? "成功" : "失败");

            if (isSuccess is not true)
            {
                await base._bus.RaiseEvent(new DomainNotification("", errorMsg));
                return await Task.FromResult(new Unit());
            }

            _logger.LogInformation("发送命令，下一步将sku数据保存一份到es");
            await base._bus.SendCommand(new SkuInfoPersistentToEsCommand(skus)); // 发送命令，将sku数据保存到es

            return await Task.FromResult(new Unit());
        }

        /// <summary>
        /// 将sku数据持久化到es中
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Unit> Handle(SkuInfoPersistentToEsCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("准备开始将sku数据保存至es");
            await _skuEsService.SaveSkuData(request.Skus);

            await base._bus.RaiseEvent(new AddProductEvent());
            return await Task.FromResult(new Unit());
        }
    }
}
