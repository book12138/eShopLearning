using eShopLearning.WapAggregator;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

string _namespace = typeof(Startup).Namespace;
string _appName = _namespace.Substring(_namespace.LastIndexOf('.', _namespace.LastIndexOf('.') - 1) + 1);

#region 定义 IConfiguration 实例获取方法
IConfiguration GetConfiguration()
    => (new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddEnvironmentVariables()).Build();
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
        .ConfigureAppConfiguration(x => x.AddConfiguration(configuration))
        .UseStartup<Startup>()
        .UseContentRoot(Directory.GetCurrentDirectory())
        .UseSerilog()
        .Build();

var host = BuildWebHost(configuration, args);

host.Run(); // 启动应用
