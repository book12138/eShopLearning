using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.ProductSlaveDbHealthCheck.Quartz
{
    public class JobSchedule
    {
        public JobSchedule(Type jobType, string cronExpression)
        {
            JobType = jobType; 
            CronExpression = cronExpression;
        }

        /// <summary>
        /// 作业类型
        /// </summary>
        public Type JobType { get; }
        /// <summary>
        /// cron 表达式
        /// </summary>
        public string CronExpression { get; }
    }
}
