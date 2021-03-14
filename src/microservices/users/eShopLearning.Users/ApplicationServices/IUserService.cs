using eShopLearning.Users.Dto;
using eShopLearning.Users.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.Users.ApplicationServices
{
    public interface IUserService
    {
        /// <summary>
        /// 添加用户
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        Task<ResponseModel> UserAdd(UserAddDto dto);
        /// <summary>
        /// 密码核对并在确认核对正确的情况下，返回用户信息
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        Task<ResponseModel<UserPasswordCheckResultWithUserInfoDto>> UserPasswordCheck(UserPasswordCheckDto dto);
        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="userId">用户id</param>
        /// <param name="originalPassword">原密码</param>
        /// <param name="newPassword">新密码</param>
        /// <returns></returns>
        Task<ResponseModel> UserPasswordModify(long userId, string originalPassword, string newPassword);

    }
}
