using eShopLearning.Users.ApplicationServices;
using eShopLearning.Users.Dto;
using eShopLearning.Common;
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
    /// 账户信息服务
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        /// <summary>
        /// 用户应用服务
        /// </summary>
        private readonly IUserService _userService;

        /// <summary>
        /// 构造注入
        /// </summary>
        /// <param name="userService"></param>
        public AccountController(IUserService userService)
            => _userService = userService;

        /// <summary>
        /// 注册用户
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("Register")]
        public async Task<ResponseModel> Register([FromBody] UserAddDto dto)
            => await _userService.UserAdd(dto);

        /// <summary>
        /// 密码校验
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("PasswordCheck")]
        public async Task<ResponseModel<UserPasswordCheckResultWithUserInfoDto>> PasswordCheck([FromBody] UserPasswordCheckDto dto)
            => await _userService.UserPasswordCheck(dto);

        /// <summary>
        /// 密码修改
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPut("PasswordModify")]
        [Authorize]
        public async Task<ResponseModel> PasswordModify([FromBody] UserPasswordModifyDto dto)
            => await _userService.UserPasswordModify(dto.UserId, dto.OriginalPassword, dto.NewPassword);
    }
}
