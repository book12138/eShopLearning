using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.Products.Domain.Base
{
    /// <summary>
    /// 领域事件模型抽象基类
    /// </summary>
    public abstract class DomainEvent : Message, INotification
    {
        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime TimeStamp { get; }

        protected DomainEvent() => this.TimeStamp = DateTime.Now;
    }
}
