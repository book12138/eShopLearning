using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.Products.Domain.Base
{
    /// <summary>
    /// 抽象命令基类
    /// </summary>
    public abstract class DomainCommand : Message
    {
        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; private set; }

        protected DomainCommand()
        {
            Timestamp = DateTime.Now;
        }
    }
}
