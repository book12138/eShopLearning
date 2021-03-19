using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.Products.Domain.Base
{
    public abstract class Message : IRequest
    {
        /// <summary>
        /// 类名
        /// </summary>
        public string MessageType { get; protected set; }
        /// <summary>
        /// 聚合根id
        /// </summary>
        public string AggregateId { get; protected set; }

        protected Message()
        {
            MessageType = GetType().Name;
        }
    }
}
