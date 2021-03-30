using AutoMapper;
using eShopLearning.Users.ApplicationServices;
using eShopLearning.Users.ApplicationServices.Impl;
using eShopLearning.Users.AutoMapper;
using eShopLearning.Users.Dto;
using eShopLearning.Users.EFCoreRepositories.EFCore;
using eShopLearning.Users.EFCoreRepositories.Entities;
using eShopLearning.Users.EFCoreRepositories.Repositories;
using eShopLearning.Users.EFCoreRepositories.Repositories.Impl;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Org.BouncyCastle.Crypto.Tls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace eShopLearning.UsersTesting
{
    public class UserServiceTest
    {
        private readonly IUserService _userService;
        private readonly ApplicationUserDbContext _applicationUserDbContext;        

        public UserServiceTest()
        {
            var services = new ServiceCollection();

            services.AddEntityFrameworkMySql() // ef core in memory
                .AddDbContext<ApplicationUserDbContext>(options => options.UseInMemoryDatabase("test"));
            services.AddAutoMapper(typeof(CustomProfile)); // automapper
            services.AddTransient<IUserRepository, UserRepository>(); // 用户仓储
            services.AddTransient<IUserService, UserService>(); // 用户服务
            services.AddTransient<IBCryptService, BCryptService>(); // 加密服务

            var serviceProvider = services.BuildServiceProvider();
            _userService = serviceProvider.GetService<IUserService>();
            _applicationUserDbContext = serviceProvider.GetService<ApplicationUserDbContext>();


            #region ef core 上下文初始化一些数据

            var bCryptService = serviceProvider.GetService<IBCryptService>();
            _applicationUserDbContext.Users.Add(new User { Username = "shapman", Password = bCryptService.HashPassword("123"), Avatar = "default.jpg", Enable = true, NickName = "吕小不", Status = true });
            _applicationUserDbContext.SaveChanges();

            #endregion
        }

        #region 用户添加

        /// <summary>
        /// 添加一个已存在的用户
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddAnExistingUser()
        {
            var result = await _userService.UserAdd(new UserAddDto { Username = "shapman", Password = "123" });
            result.Code.Should().Be(1001);
        }

        /// <summary>
        /// 添加一个不存在的用户
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddAnNonExistentUser()
        {           
            var result = await _userService.UserAdd(new UserAddDto { Username = "sample", Password = "123" });
            result.Code.Should().Be(200);
        }

        #endregion

        #region 密码校验

        /// <summary>
        /// 使用正确的密码进行校验
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UseCorrectPasswprdToCheck()
        {
            var result = await _userService.UserPasswordCheck(new UserPasswordCheckDto { Username = "shapman", Password = "123" });
            result.Code.Should().Be(200);
            result.Data.IsTrue.Should().BeTrue();
            result.Data.UserInfo.Should().NotBeNull();
            result.Data.UserInfo.Id.Should().Be(1);
        }

        /// <summary>
        /// 使用错误的密码进行校验
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UseWrongPasswordToCheck()
        {
            var result = await _userService.UserPasswordCheck(new UserPasswordCheckDto { Username = "shapman", Password = "321" });
            result.Code.Should().Be(200);
            result.Data.IsTrue.Should().BeFalse();
            result.Data.UserInfo.Should().BeNull();
        }

        /// <summary>
        /// 使用不存在的用户名去校验
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UseNonExistentUsernameToCheck()
        {
            var result = await _userService.UserPasswordCheck(new UserPasswordCheckDto { Username = "sample", Password = "321" });
            result.Code.Should().Be(1001);
        }

        #endregion

        #region 用户信息获取
        /// <summary>
        /// 使用已经存在的用户id测试用户信息获取
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UseExistUserIdToGetUserInfoTest()
        {
            var result = await _userService.GetUserInfo(1);
            result.Code.Should().Be(200);
            result.Data.Username.Should().Be("shapman");
        }

        /// <summary>
        /// 使用吧不存在的用户id去尝试获取用户信息
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UseNonExistUserIdToGetUserInfoTest()
        {
            var result = await _userService.GetUserInfo(2);
            result.Code.Should().Be(400);
            result.Data.Should().BeNull();
        }
        #endregion
    }
}
