using eShopLearning.Products.Domain.Bus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.Products.Domain.Base
{
    /// <summary>
    /// 领域命令处理基类
    /// </summary>
    public class DomainCommandHandler
    {
        /// <summary>
        /// 中介处理总线
        /// </summary>
        protected readonly IApplicationBus _bus;

        /// <summary>
        /// 构造注入
        /// </summary>
        /// <param name="memoryCache"></param>
        public DomainCommandHandler(IApplicationBus mediatorHandler) => this._bus = mediatorHandler;

        ///// <summary>
        ///// 领域验证所产生的错误消息的统一收集
        ///// </summary>
        ///// <param name="command">命令模型</param>
        //protected void NotifyValidationErrorCollection(DomainCommand command)
        //{
        //    List<string> errorInfo = new List<string>();
        //    foreach (var error in command.ValidationResult.Errors)
        //    {
                
        //        _bus.RaiseEvent(new DomainNotification("", error.ErrorMessage)); // 将错误信息提交到事件总线，派发出去
        //    }
        //}
    }
}
