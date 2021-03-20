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
        IRequestHandler<AddProductCommand>
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
        /// 
        /// </summary>
        /// <param name="mediatorHandler"></param>
        /// <param name="productService"></param>
        /// <param name="logger"></param>
        public ProductCommandHandler(
            IApplicationBus mediatorHandler,
            IProductService productService,
            ILogger<ProductCommandHandler> logger
            )
            :base(mediatorHandler)
        {
            this._productService = productService;
            this._logger = logger;
        }

        /// <summary>
        /// 产品添加命令处理程序
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Unit> Handle(AddProductCommand request, CancellationToken cancellationToken)
        {
            (bool isSuccess, string errorMsg, IEnumerable<Sku> skus) = await this._productService.AddProduct(request.Category, request.SkuDtos);

            if (isSuccess is not true)
            {
                await base._bus.RaiseEvent(new DomainNotification("", errorMsg));
                return await Task.FromResult(new Unit());
            }


            await base._bus.RaiseEvent(new AddProductEvent(skus));

            return await Task.FromResult(new Unit());
        }
    }
}
