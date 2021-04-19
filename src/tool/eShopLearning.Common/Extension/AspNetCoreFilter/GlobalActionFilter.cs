using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace eShopLearning.Common.Extension.AspNetCoreFilter
{
    /// <summary>
    /// action 全局监视
    /// </summary>
    public class GlobalActionFilter : ActionFilterAttribute
    {
        /// <summary>
        /// 日志
        /// </summary>
        private readonly ILogger _log;
        /// <summary>
        /// 定义不要打日志的接口集合
        /// </summary>
        private static HashSet<string> _doNotLogApis = new HashSet<string>(new[]{
            "/Health/Check"
        });
        /// <summary>
        /// 定义不要对响应内容打日志的接口集合
        /// </summary>
        private static HashSet<string> _doNotLogResponseContentApis = new HashSet<string>(new[]{
            "/Product/Search",
            "/CartProduct/GetUserCartProducts"
        });

        /// <summary>
        /// 构造 
        /// </summary>
        /// <param name="logger"></param>
        public GlobalActionFilter(ILogger<GlobalActionFilter> logger) => _log = logger;

        /// <summary>
        /// 准备进入action
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            foreach (var item in context.ActionArguments)
            {
                var jsonStr = JsonConvert.SerializeObject(item.Value);
                var newJsonStr = Regex.Replace(jsonStr, "(\"\\s+)|(\\s+\")", "\""); // 去掉字符串类型参数前后空格

                if (jsonStr.Length != newJsonStr.Length) // 检查参数字符串在去除掉了空格之后，是否和原来的不一样
                {
                    object newValueObj = null;
                    using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(newJsonStr)))
                    {
                        DataContractJsonSerializer deseralizer = new DataContractJsonSerializer(item.Value.GetType());
                        newValueObj = deseralizer.ReadObject(ms);
                    }
                    Type type = item.Value.GetType();
                    foreach (var prop in newValueObj.GetType().GetProperties()) // 无法直接针对 item.Value 进行反射赋值操作，所以再往深一层进行遍历赋值
                    {
                        var field = type.GetField($"<{prop.Name}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
                        field?.SetValue(item.Value, prop.GetValue(newValueObj));
                    }

                }
            }

            var apiPath = context?.HttpContext?.Request?.Path;
            var endpoint = context.HttpContext.Features.Get<IEndpointFeature>()?.Endpoint;
            var controllerName = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>().ControllerName;
            var actionName = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>().ActionName;

            if (_doNotLogApis.Contains($"/{controllerName}/{actionName}") is false)
                _log.LogInformation("" +
                   "【当前请求接口】：{apiPath} \r\n" +
                   "【携带的参数有】： {args}", apiPath, JsonConvert.SerializeObject(context.ActionArguments));
            base.OnActionExecuting(context);
        }

        /// <summary>
        /// 在请求执行了 action， 已获得result
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            string apiPath = context.HttpContext.Request.Path;
            var endpoint = context.HttpContext.Features.Get<IEndpointFeature>()?.Endpoint;
            var controllerName = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>().ControllerName;
            var actionName = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>().ActionName;

            if (_doNotLogApis.Contains($"/{controllerName}/{actionName}") is true)
            {
                base.OnActionExecuted(context);
                return;
            }

            if (_doNotLogResponseContentApis.Contains($"/{controllerName}/{actionName}") is false)
                _log.LogInformation("" +
                   "【当前响应接口】：{apiPath} \r\n" +
                   "【接口返回内容】： {response}", apiPath, JsonConvert.SerializeObject(context.Result));
            else
                _log.LogInformation("" +
                    "【当前响应接口】：{responsePath} \r\n" +
                    "【接口返回内容】： {response}", context.HttpContext.Request.Path, "{ 系统内部已被设置为不显示 }");

            base.OnActionExecuted(context);
        }
    }
}
