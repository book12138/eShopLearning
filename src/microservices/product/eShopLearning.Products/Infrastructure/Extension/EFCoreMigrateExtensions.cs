using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.Products.Infrastructure.Extension
{
    public static class EFCoreMigrateExtensions
    {
        /// <summary>
        /// 当前是否运行在 k8s 中
        /// </summary>
        /// <param name="webHost"></param>
        /// <returns></returns>
        public static bool IsInKubernetes(this IWebHost webHost)
        {            
            var config = webHost.Services.GetService(typeof(IConfiguration)) as IConfiguration;
            var orchestratorType = config.GetValue<string>("OrchestratorType"); // 获取协调器类型配置
            return orchestratorType?.ToUpper() == "K8S";
        }

        /// <summary>
        /// EF Core 迁移（内置数据库瞬态异常处理，k8s下为直接抛出异常）
        /// </summary>
        /// <typeparam name="TContext">具体的DbContext</typeparam>
        /// <param name="webHost"></param>
        /// <param name="seeder">种子数据具体的迁移策略</param>
        /// <returns></returns>
        public static IWebHost MigrateDbContext<TContext>(this IWebHost webHost, Action<TContext, IServiceProvider> seeder) where TContext : DbContext
        {
            var underK8s = webHost.IsInKubernetes(); // 是否在 k8s 上

            using (var scope = webHost.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<TContext>>();
                var context = services.GetService<TContext>();

                try
                {
                    logger.LogInformation("开始针对 {DbContextName} 进行迁移", typeof(TContext).Name);

                    if (underK8s)
                    {
                        InvokeSeeder(seeder, context, services); // 是在 k8s 上部署运行的，执行迁移
                    }
                    else // 非 k8s 部署运行，使用 Polly 做一些重试策略
                    {
                        Policy.Handle<Exception>() // 定义瞬态故障处理策略
                            .WaitAndRetry(
                                retryCount: 10, // 定义重试次数
                                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                                onRetry: (exception, timeSpan, retry, ctx) => // 定义发生重试时，需要执行的一些操作
                                    logger.LogWarning(exception, "针对 {DbContextName} 进行迁移时发生错误，准备重试", nameof(TContext))
                                )
                            .Execute(() => InvokeSeeder(seeder, context, services)); // 执行迁移，并使用 Polly 包裹这一操作
                    }

                    logger.LogInformation("针对 {DbContextName} 的迁移结束", typeof(TContext).Name);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "当针对 {DbContextName} 进行迁移时发生错误", typeof(TContext).Name);
                    if (underK8s) // 在 k8s 上有相应重试策略，所以如果是在 k8s 上，就直接把错误抛出去
                        throw;
                }
            }

            return webHost;
        }

        /// <summary>
        /// 迁移种子数据
        /// </summary>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="seeder">种子数据迁移所对应的handle</param>
        /// <param name="context">具体的DbContext</param>
        /// <param name="services">dotnet core依赖注入</param>
        private static void InvokeSeeder<TContext>(Action<TContext, IServiceProvider> seeder, TContext context, IServiceProvider services)
            where TContext : DbContext
        {
            context.Database.Migrate(); // 数据库表迁移
            seeder(context, services); // 种子数据迁移
        }
    }
}
