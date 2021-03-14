using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace eShopLearning.Users.Aop
{
    /// <summary>
    /// 全局异常处理
    /// </summary>
    public class GlobalExceptionFilter : ExceptionFilterAttribute
    {
        /// <summary>
        /// 日志
        /// </summary>
        private readonly ILogger _log;

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="logger"></param>
        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
        {
            _log = logger;
        }

        /// <summary>
        /// 当系统出现异常时
        /// </summary>
        /// <param name="context"></param>
        public override void OnException(ExceptionContext context)
        {
            _log.LogError(context.Exception.StackTrace);
        }
    }
}
