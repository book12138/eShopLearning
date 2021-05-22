using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Threading.Tasks;

namespace eShopLearning.ProductSlaveDbHealthCheck.Jobs
{
    /// <summary>
    /// 商品从库健康检查
    /// </summary>
    public class ProductSlaveDbHealthCheckJob : IJob
    {
        /// <summary>
        /// 日志
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// 定时任务的执行入口
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task Execute(IJobExecutionContext context)
        {
            throw new NotImplementedException();
        }
    }
}
