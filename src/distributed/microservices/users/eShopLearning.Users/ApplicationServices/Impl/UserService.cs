using AutoMapper;
using Microsoft.EntityFrameworkCore;
using eShopLearning.Users.EFCoreRepositories.EFCore;
using eShopLearning.Users.EFCoreRepositories.Entities;
using eShopLearning.Users.EFCoreRepositories.Repositories;
using System.Threading.Tasks;
using eShopLearning.Users.Dto;
using eShopLearning.Common;

namespace eShopLearning.Users.ApplicationServices.Impl
{
    public class UserService : IUserService
    {        
        /// <summary>
        /// 用户仓储
        /// </summary>
        private readonly IUserRepository _userRepository;       
        /// <summary>
        /// automapper
        /// </summary>
        private readonly IMapper _mapper;
        /// <summary>
        /// EF Core
        /// </summary>
        private readonly ApplicationUserDbContext _applicationUserContext;
        /// <summary>
        /// 加密
        /// </summary>
        private readonly IBCryptService _bCryptService;

        /// <summary>
        /// 构造注入
        /// </summary>
        /// <param name="userRepository"></param>
        /// <param name="mapper"></param>
        /// <param name="applicationUserContext"></param>
        /// <param name="bCryptService"></param>
        public UserService(
            IUserRepository userRepository,
            IMapper mapper,
            ApplicationUserDbContext applicationUserContext,
            IBCryptService bCryptService
            )
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _applicationUserContext = applicationUserContext;
            _bCryptService = bCryptService;
        }

        /// <summary>
        /// 添加用户
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<ResponseModel> UserAdd(UserAddDto dto)
        {
            if (await _applicationUserContext.Users.AnyAsync(u => u.Username.Trim() == dto.Username.Trim())) // 删除的用户，其用户名一样不准被重复
                return new ResponseModel { Code = 1001, Msg = "此用户名已被注册" };

            await _userRepository.AddAsync(new User { Username = dto.Username, Password = _bCryptService.HashPassword(dto.Password) });
            await _applicationUserContext.SaveChangesAsync();
            return ResponseModel.BuildResponse(PublicStatusCode.Success);
        }

        /// <summary>
        /// 密码核对
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<ResponseModel<UserPasswordCheckResultWithUserInfoDto>> UserPasswordCheck(UserPasswordCheckDto dto)
        {
            var user = await _applicationUserContext.Users.FirstOrDefaultAsync(u => u.Status && u.Username.Trim() == dto.Username.Trim());
            if (user == null)
                return new ResponseModel<UserPasswordCheckResultWithUserInfoDto> { Code = 1001, Msg = "该用户不存在" };

            if (_bCryptService.Verify(dto.Password.Trim(), user.Password)) // 验证通过
                return new ResponseModel<UserPasswordCheckResultWithUserInfoDto> { Code = 200, Data = new UserPasswordCheckResultWithUserInfoDto { IsTrue = true, UserInfo = _mapper.Map<UserInfoDto>(user) } };

            return new ResponseModel<UserPasswordCheckResultWithUserInfoDto> { Code = 200, Data = new UserPasswordCheckResultWithUserInfoDto { IsTrue = false } };
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="userId">用户id</param>
        /// <param name="originalPassword">原密码</param>
        /// <param name="newPassword">新密码</param>
        /// <returns></returns>
        public async Task<ResponseModel> UserPasswordModify(long userId,string originalPassword, string newPassword)
        {
            var user = await _applicationUserContext.Users.FirstOrDefaultAsync(u => u.Status && u.Id == userId);
            if (user == null)
                return new ResponseModel { Code = 1001, Msg = "此用户不存在" };

            if (!_bCryptService.Verify(originalPassword, user.Password))
                return new ResponseModel { Code = 1002, Msg = "原密码错误" };

            user.Password = _bCryptService.HashPassword(newPassword);
            await _applicationUserContext.SaveChangesAsync();
            return ResponseModel.BuildResponse(PublicStatusCode.Success);
        }

        /// <summary>
        /// 按照用户id获取用户信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<ResponseModel<UserInfoDto>> GetUserInfo(long userId)
        {
            var user = await _applicationUserContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return ResponseModel<UserInfoDto>.BuildResponse(PublicStatusCode.Fail, "");
            else return ResponseModel<UserInfoDto>.BuildResponse(PublicStatusCode.Success, _mapper.Map<UserInfoDto>(user));
        }
    }
}
