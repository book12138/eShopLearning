using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace eShopLearning.IdentityAuth.Interstructure
{
    /// <summary>
    /// 接口返回报文统一包装（不带数据返回）
    /// </summary>
    public class ResponseModel
    {
        /// <summary>
        /// 状态码
        /// 200成功，400错误，500系统错误
        /// 1001开始为每个接口的自定义状态码
        /// 默认500
        /// </summary>
        public int Code { get; set; } = 500;
        /// <summary>
        /// 消息
        /// </summary>
        public string Msg { get; set; } = "";
        /// <summary>
        /// 按照状态枚举创建回返报文
        /// </summary>
        /// <param name="en"></param>
        /// <returns></returns>
        public static ResponseModel BuildResponse(PublicStatusCode en)
            => new ResponseModel
            {
                Code = (int)en,
                Msg = en.GetType().GetCustomAttribute<DisplayAttribute>()?.Name ?? ""
            };

        /// <summary>
        /// 按照状态枚举修改回返报文
        /// </summary>
        /// <param name="en"></param>
        public void UpdateCodeAndMsg(PublicStatusCode en)
        {
            Code = (int)en;
            Msg = en.GetType().GetCustomAttribute<DisplayAttribute>()?.Name ?? "";
        }
    }

    /// <summary>
    /// 常用的一些状态枚举
    /// </summary>
    public enum PublicStatusCode
    {
        /// <summary>
        /// 成功
        /// </summary>
        [Display(Name = "成功")]
        Success = 200,
        /// <summary>
        /// 失败
        /// </summary>
        [Display(Name = "失败")]
        Fail = 400,
        /// <summary>
        /// 系统错误
        /// </summary>
        [Display(Name = "系统错误")]
        SystemError = 500,
    }

    /// <summary>
    /// 接口返回报文统一包装（带数据返回）
    /// </summary>
    public class ResponseModel<T> : ResponseModel
    {
        /// <summary>
        /// 返回的数据
        /// </summary>
        public T Data { get; set; }
    }
}
