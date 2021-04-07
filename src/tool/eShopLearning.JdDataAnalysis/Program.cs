using AutoMapper;
using eShopLearning.JdDataAnalysis;
using eShopLearning.JdDataAnalysis.ApplicationServices;
using eShopLearning.JdDataAnalysis.ApplicationServices.Impl;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.IO;

/*
 *   ******* 京东爬虫 *******
 *
 * 京东有点严格，需要自行实现动态代理，
 * 否则多抓几次数据就会被强制跳到登录页
 */

#region ioc setting
IServiceProvider ConfigureServices()
{
    var services = new ServiceCollection();

    services.AddTransient<IConfiguration>(u => {
        var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true) // 设置配置文件路径
        .AddUserSecrets("bcdeadae-9673-4223-b42a-0d54bf981cc7");

        return builder.Build();
    });
    var configuration = services.BuildServiceProvider().GetService<IConfiguration>();

    // log
    services.AddLogging(build =>
    {
        build.AddConfiguration(configuration.GetSection("Logging"));
        build.AddConsole();
    });

    // automapper
    services.AddAutoMapper(typeof(CustomProfile));

    services.AddTransient<IProductDataGrabService, ProductDataGrabService>();
    services.AddTransient<IDataPersistenceService, DataPersistenceService>();
    //services.AddTransient<IDataPersistenceService, SkuDataPublishToRabbitMQ>();
    services.AddTransient<IProxyIpService, ProxyIpService>();

    // rabbitmq
    services.AddTransient<IConnectionFactory>(u => new ConnectionFactory() { 
        HostName = configuration["RabbitMQ:HostName"], 
        UserName = configuration["RabbitMQ:UserName"],
        Password = configuration["RabbitMQ:Password"],  
        Port = int.TryParse(configuration["RabbitMQ:Port"], out int parseResult) ? parseResult : 5672
    });

    return services.BuildServiceProvider();
}
#endregion

var _serviceProvider = ConfigureServices();
var _productDataGrabService = _serviceProvider.GetService(typeof(IProductDataGrabService)) as IProductDataGrabService;
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("嗨，爬虫出发 " + DateTime.Now);

//await _productDataGrabService.GrabDataFromSearchPage("短袖T恤 男", "6778073159441805314", 1);
await _productDataGrabService.GrabDataFromSearchPage("小西装 女", "6778073159441805320", 1);

Console.WriteLine("按下任意键继续......");
Console.ReadKey();