using eShopLearning.Common;
using eShopLearning.Users.ApplicationServices;
using eShopLearning.Users.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.Users.Controllers
{
    /// <summary>
    /// 用户信息服务
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UserInfoController : ControllerBase
    {
        /// <summary>
        /// 用户应用服务
        /// </summary>
        private readonly IUserService _userService;

        /// <summary>
        /// 构造注入
        /// </summary>
        /// <param name="userService"></param>
        public UserInfoController(IUserService userService)
            => _userService = userService;

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("GetUserInfo/[id]")]
        [Authorize]
        public async Task<ResponseModel<UserInfoDto>> GetUserInfo(long id)
            => await _userService.GetUserInfo(id);

        /// <summary>
        /// 获取个人信息(仅支持token里面包含了id的)
        /// </summary>
        /// <returns></returns>        
        [HttpGet("GetUserInfo")]
        [Authorize]
        public async Task<ResponseModel<UserInfoDto>> GetUserInfo()
        {
            return await _userService.GetUserInfo(1);
        }
    }
}
