using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace eShopLearning.Products.Domain.Bus
{
    /// <summary>
    /// 领域通知处理程序，把所有的通知信息放到事件总线中
    /// </summary>
    public class DomainNotificationHandler : INotificationHandler<DomainNotification>
    {
        /// <summary>
        /// 通知信息列表
        /// </summary>
        private List<DomainNotification> _notifications;

        public DomainNotificationHandler() => this._notifications = new List<DomainNotification>();

        /// <summary>
        /// 处理方法，把全部的通知信息都放到缓存
        /// </summary>
        /// <param name="notification">通知</param>
        /// <param name="cancellationToken">取消标记</param>
        /// <returns></returns>
        public Task Handle(DomainNotification notification, CancellationToken cancellationToken)
        {
            this._notifications.Add(notification);//储存
            return Task.CompletedTask;
        }

        /// <summary>
        /// 提取当前生命周期内所有通知信息
        /// </summary>
        /// <returns></returns>
        public virtual List<DomainNotification> GetNotifications() => this._notifications;

        /// <summary>
        /// 确定是否存在通知消息
        /// </summary>
        /// <returns></returns>
        public virtual bool HasNotifications() => this.GetNotifications().Any();

        /// <summary>
        /// 手动回收 清空通知消息
        /// </summary>
        public void Dispose() => this._notifications = new List<DomainNotification>();
    }
}
