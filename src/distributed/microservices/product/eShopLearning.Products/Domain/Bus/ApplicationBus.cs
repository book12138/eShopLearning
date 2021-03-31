using eShopLearning.Products.Domain.Base;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.Products.Domain.Bus
{
    /// <summary>
    /// 程序应用中介总线（命令总线、事件总线）
    /// </summary>
    public sealed class ApplicationBus : IApplicationBus
    {
        /// <summary>
        /// 总线
        /// </summary>
        private readonly IMediator _mediator;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="mediator"></param>
        public ApplicationBus(IMediator mediator) => this._mediator = mediator;

        /// <summary>
        /// 事件的发布
        /// </summary>
        /// <typeparam name="T">事件类型</typeparam>
        /// <param name="event">事件模型</param>
        /// <returns></returns>
        public Task RaiseEvent<T>(T @event) where T : DomainEvent => _mediator.Publish(@event); 

        /// <summary>
        /// 发送命令
        /// </summary>
        /// <typeparam name="T">命令类型</typeparam>
        /// <param name="command">命令模型</param>
        /// <returns></returns>
        public Task SendCommand<T>(T command) where T : DomainCommand => _mediator.Send(command);
    }
}
