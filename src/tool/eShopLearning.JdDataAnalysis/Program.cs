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

/**
 *                             _ooOoo_
 *                            o8888888o
 *                            88" . "88
 *                            (| -_- |)
 *                            O\  =  /O
 *                         ____/`---'\____
 *                       .'  \\|     |//  `.
 *                      /  \\|||  :  |||//  \
 *                     /  _||||| -:- |||||-  \
 *                     |   | \\\  -  /// |   |
 *                     | \_|  ''\---/''  |   |
 *                     \  .-\__  `-`  ___/-. /
 *                   ___`. .'  /--.--\  `. . __
 *                ."" '<  `.___\_<|>_/___.'  >'"".
 *               | | :  `- \`.;`\ _ /`;.`/ - ` : | |
 *               \  \ `-.   \_ __\ /__ _/   .-` /  /
 *          ======`-.____`-.___\_____/___.-`____.-'======
 *                             `=---='
 *          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
 *                     佛祖保佑        永无BUG
 *            佛曰:
 *                   写字楼里写字间，写字间里程序员；
 *                   程序人员写程序，又拿程序换酒钱。
 *                   酒醒只在网上坐，酒醉还来网下眠；
 *                   酒醉酒醒日复日，网上网下年复年。
 *                   但愿老死电脑间，不愿鞠躬老板前；
 *                   奔驰宝马贵者趣，公交自行程序员。
 *                   别人笑我忒疯癫，我笑自己命太贱；
 *                   不见满街漂亮妹，哪个归得程序员？
*/


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