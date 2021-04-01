using eShopLearning.Products.Domain.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.Products.Domain.Bus
{
    public interface IApplicationBus
    {
        /// <summary>
        /// 发布命令，将的命令模型发布到中介者模块
        /// </summary>
        /// <typeparam name="T"> 命令类型 </typeparam>
        /// <param name="command"> 命令模型 </param>
        /// <returns></returns>
        Task SendCommand<T>(T command) where T : DomainCommand;
        /// <summary>
        /// 通过事件总线发布事件
        /// </summary>
        /// <typeparam name="T">事件类型，需继承自领域事件基类</typeparam>
        /// <param name="event">事件模型</param>
        /// <returns></returns>
        Task RaiseEvent<T>(T @event) where T : DomainEvent;
    }
}
