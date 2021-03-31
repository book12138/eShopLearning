using eShopLearning.Users.ApplicationServices;
using eShopLearning.Users.EFCoreRepositories.Entities;
using eShopLearning.Common.Extension;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace eShopLearning.Users.EFCoreRepositories.EFCore
{
    /// <summary>
    /// EF Core 种子数据迁移
    /// </summary>
    public class ApplicationUserDbContextSeed
    {
        /// <summary>
        /// 密码加密
        /// </summary>
        private readonly IBCryptService _bCryptService;
        /// <summary>
        /// 系统配置读取
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// 构造注入
        /// </summary>
        /// <param name="bCryptService"></param>
        /// <param name="configuration"></param>
        public ApplicationUserDbContextSeed(IBCryptService bCryptService, IConfiguration configuration)
        {
            _bCryptService = bCryptService;
            _configuration = configuration;
        }

        /// <summary>
        /// 迁移种子数据
        /// </summary>
        /// <param name="context">ef core db context</param>
        /// <param name="env">主机环境</param>
        /// <param name="logger">日志</param>
        /// <param name="retry">重试次数</param>
        /// <returns></returns>
        public async Task SeedDataMigrationAsync(ApplicationUserDbContext context, IWebHostEnvironment env, ILogger<ApplicationUserDbContextSeed> logger, int? retry = 0)
        {
            int retryForAvaiability = retry.Value;

            try
            {
                var contentRootPath = env.ContentRootPath;
                var webroot = env.WebRootPath;

                if (!context.Users.Any())
                {
                    // 根据系统配置而决定是迁移代码中定义的种子数据，还是迁移csv文件中的
                    context.Users.AddRange(
                        (bool.TryParse(_configuration["AppSettings:UseCustomizationSeedData"], out bool parseResult) ? parseResult : false)
                        ? GetUsersFromFile(contentRootPath, logger) // 加载文件中的数据
                        : GetDefaultUser()); // 加载代码定义的数据

                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                if (retryForAvaiability < 10)
                {
                    retryForAvaiability++;

                    logger.LogError(ex, "当针对 {DbContextName} 进行迁移时发生错误", nameof(ApplicationUserDbContext));

                    await SeedDataMigrationAsync(context, env, logger, retryForAvaiability);
                }
            }
        }

        /// <summary>
        /// 加载迁移文件，如果文件没找到，则使用代码中定义的默认数据
        /// </summary>
        /// <param name="contentRootPath"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        private IEnumerable<User> GetUsersFromFile(string contentRootPath, ILogger logger)
        {
            string csvFileUsers = Path.Combine(contentRootPath, "EFCoreRepositories", "EFCore", "seeddata.csv"); // 迁移文件路径

            if (!File.Exists(csvFileUsers))
                return GetDefaultUser();

            string[] csvheaders;
            try
            {
                string[] requiredHeaders = { "username", "password", "nickname", "avatar", "enable", "status" };
                csvheaders = GetHeaders(requiredHeaders, csvFileUsers);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "csv文件读取发生错误，具体错误信息为: {Message}", ex.Message);

                return GetDefaultUser();
            }

            List<User> users = File.ReadAllLines(csvFileUsers)
                        .Skip(1) // skip header column
                        .Select(row => Regex.Split(row, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)"))
                        .SelectTry(column => CreateUser(column, csvheaders))
                        .OnCaughtException(ex => { logger.LogError(ex, "csv文件读取发生错误，具体错误信息为: {Message}", ex.Message); return null; })
                        .Where(x => x != null)
                        .ToList();

            return users;
        }

        /// <summary>
        /// 创建用户
        /// </summary>
        /// <param name="column"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        private User CreateUser(string[] column, string[] headers)
        {
            if (column.Count() != headers.Count())
                throw new Exception($"csv文件表头与数据的列数不一致");

            var user = new User
            {
                Username = column[Array.IndexOf(headers, "username")].Trim('"').Trim(),
                Password = column[Array.IndexOf(headers, "password")].Trim('"').Trim(),
                Avatar = column[Array.IndexOf(headers, "avatar")].Trim('"').Trim(),
                Enable = bool.TryParse(column[Array.IndexOf(headers, "enable")].Trim('"').Trim(), out bool enableParseResult) ? enableParseResult : false,
                NickName = column[Array.IndexOf(headers, "nickname")].Trim('"').Trim(),
                Status = bool.TryParse(column[Array.IndexOf(headers, "status")].Trim('"').Trim(), out bool statusParseResult) ? enableParseResult : false
            };

            user.Password = _bCryptService.HashPassword(user.Password);

            return user;
        }

        /// <summary>
        /// 获取代码中定义的默认用户
        /// </summary>
        /// <returns></returns>
        private IEnumerable<User> GetDefaultUser()
        {
            var user = new User()
            {
                Username = "jack",
                Password = "Pass@word1",
                NickName = "jack",
                Avatar = "default.jpg",
                Enable = true,
                Status = true
            };

            user.Password = _bCryptService.HashPassword(user.Password);

            return new List<User> { user };
        }

        /// <summary>
        /// 解析 csv 文件中表头
        /// </summary>
        /// <param name="requiredHeaders"></param>
        /// <param name="csvfile"></param>
        /// <returns></returns>
        static string[] GetHeaders(string[] requiredHeaders, string csvfile)
        {
            string[] csvheaders = File.ReadLines(csvfile).First().ToLowerInvariant().Split(',');

            if (csvheaders.Count() != requiredHeaders.Count())
            {
                throw new Exception($"csv文件读取后得到的表头数目不是 '{ requiredHeaders.Count()}'，而是 '{csvheaders.Count()}'");
            }

            foreach (var requiredHeader in requiredHeaders)
            {
                if (!csvheaders.Contains(requiredHeader))
                {
                    throw new Exception($"csv文件的表头 '{requiredHeader}' 不存在");
                }
            }

            return csvheaders;
        }
    }
}
