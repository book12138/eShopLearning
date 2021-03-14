using AutoMapper;
using eShopLearning.JdDataAnalysis;
using eShopLearning.JdDataAnalysis.ApplicationServices;
using eShopLearning.JdDataAnalysis.ApplicationServices.Impl;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true); // 设置配置文件路径

        return builder.Build();
    });

    // log
    services.AddLogging(build =>
    {
        build.AddConfiguration(services.BuildServiceProvider().GetService<IConfiguration>().GetSection("Logging"));
        build.AddConsole();
    });

    // automapper
    services.AddAutoMapper(typeof(CustomProfile));

    services.AddTransient<IProductDataGrabService, ProductDataGrabService>();
    services.AddTransient<IDataPersistenceService, DataPersistenceService>();
    services.AddTransient<IProxyIpService, ProxyIpService>();

    return services.BuildServiceProvider();
}
#endregion

var _serviceProvider = ConfigureServices();
var _productDataGrabService = _serviceProvider.GetService(typeof(IProductDataGrabService)) as IProductDataGrabService;
Console.ForegroundColor = ConsoleColor.Cyan;

// _productDataGrabService.GrabDataFromSearchPage("女衣", 2);

Console.WriteLine("嗨，现在时间为：" + DateTime.Now);

Console.WriteLine("按下任意键继续......");
Console.ReadKey();