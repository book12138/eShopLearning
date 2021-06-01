using eShopLearning.Users;
using eShopLearning.Users.ApplicationServices;
using eShopLearning.Users.EFCoreRepositories.EFCore;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.IO;
using eShopLearning.Common.Extension;
using eShopLearning.Common.Extension.Configuration;
using Com.Ctrip.Framework.Apollo;

string _namespace = typeof(Startup).Namespace;
string _appName = _namespace.Substring(_namespace.LastIndexOf('.', _namespace.LastIndexOf('.') - 1) + 1);

#region 定义 IConfiguration 实例获取方法
IConfiguration GetConfiguration()
    => (new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .TryAddJsonFile("appsettings.Development.json", optional: false, reloadOnChange: true) // 尝试把多环境配置文件一同加载进来
        .TryAddJsonFile("appsettings.Staging.json", optional: false, reloadOnChange: true)
        .TryAddJsonFile("appsettings.Production.json", optional: false, reloadOnChange: true)
        .AddEnvironmentVariables())
        .Build();
#endregion

#region 定义 serilog
Serilog.ILogger CreateSerilogLogger(IConfiguration configuration)
    => new LoggerConfiguration()
        .MinimumLevel.Verbose()
        .Enrich.WithProperty("ApplicationContext", _appName)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.Http(configuration["Serilog:LogstashgUrl"] ?? "http://localhost:8080") // ELK + Serilog 实现日志中心
        .ReadFrom.Configuration(configuration)
        .CreateLogger();
#endregion

var configuration = GetConfiguration();
Log.Logger = CreateSerilogLogger(configuration);
IWebHost BuildWebHost(IConfiguration configuration, string[] args) =>
    WebHost.CreateDefaultBuilder(args)
        .CaptureStartupErrors(false)
           .ConfigureAppConfiguration(x => {
               x.AddConfiguration(configuration);
               x.AddApollo(configuration.GetSection("Apollo")).AddDefault(); // apollo config
           })
        .UseStartup<Startup>()
        .UseContentRoot(Directory.GetCurrentDirectory())
        .UseSerilog()
        .Build();

var host = BuildWebHost(configuration, args);

#region ef core 迁移
host.MigrateDbContext<ApplicationUserDbContext>((context, services) =>
        {
            var env = services.GetService(typeof(IWebHostEnvironment)) as IWebHostEnvironment;
            var logger = services.GetService(typeof(ILogger<ApplicationUserDbContextSeed>)) as ILogger<ApplicationUserDbContextSeed>;
            var bCryptService = services.GetService(typeof(IBCryptService)) as IBCryptService;

            new ApplicationUserDbContextSeed(bCryptService, configuration)
                .SeedDataMigrationAsync(context, env, logger).Wait(); // 必须要用wait() ，走异步的话，dbcontext对象会被提前释放掉
        });
#endregion

host.Run(); // 启动应用
