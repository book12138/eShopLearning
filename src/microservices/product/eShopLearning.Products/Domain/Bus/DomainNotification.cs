using eShopLearning.Products.Domain.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.Products.Domain.Bus
{
    /// <summary>
    /// 领域通知模型，用于承载总线中的通知消息
    /// 继承自 领域事件基类 和 MediatR的INotification
    /// </summary>
    public class DomainNotification : DomainEvent
    {
        /// <summary>
        /// 领域通知标识
        /// </summary>
        public Guid DomainNotificationId { get; private set; }
        /// <summary>
        /// 键
        /// </summary>
        public string Key { get; private set; }
        /// <summary>
        /// 值
        /// </summary>
        public string Value { get; private set; }
        /// <summary>
        /// 版本
        /// </summary>
        public int Version { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public DomainNotification(string key, string value)
        {
            this.DomainNotificationId = Guid.NewGuid();
            this.Version = 1;
            this.Key = key;
            this.Value = value;
        }
    }
}
