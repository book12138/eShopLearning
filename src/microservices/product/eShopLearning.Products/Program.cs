using eShopLearning.Products;
using eShopLearning.Products.EFCoreRepositories.EFCore;
using eShopLearning.Common.Extension;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.IO;
using eShopLearning.Products.ApplicationServices;
using Com.Ctrip.Framework.Apollo;
using eShopLearning.Common.Extension.Configuration;

string _namespace = typeof(Startup).Namespace;
string _appName = _namespace.Substring(_namespace.LastIndexOf('.', _namespace.LastIndexOf('.') - 1) + 1);

#region 定义 IConfiguration 实例获取方法
IConfiguration GetConfiguration()
    => (new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)    
        .TryAddJsonFile("appsettings.Development.json", optional: false, reloadOnChange: true) // 尝试把多环境配置文件一同加载进来
        .TryAddJsonFile("appsettings.Production.json", optional: false, reloadOnChange: true) // 尝试把多环境配置文件一同加载进来
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
host.MigrateDbContext<eShopProductDbContext>((context, services) =>
{
    var env = services.GetService(typeof(IWebHostEnvironment)) as IWebHostEnvironment;
    var logger = services.GetService(typeof(ILogger<eShopProductDbContextSeed>)) as ILogger<eShopProductDbContextSeed>;
    var skuEsService = services.GetService(typeof(ISkuEsService)) as ISkuEsService;

    new eShopProductDbContextSeed(configuration, skuEsService)
        .SeedDataMigrationAsync(context, env, logger).Wait(); // 必须要用wait() ，走异步的话，dbcontext对象会被提前释放掉
});
#endregion

host.Run(); // 启动应用

#region 消费 rabbitmq 队列中的消息 (无法实现)
//var serviceScope = host.Services.CreateScope(); // host.Services 是主线程的，子线程里直接拿来用会报错
//var rabbitmqConnFactory = host.Services.GetService(typeof(IConnectionFactory)) as IConnectionFactory;
//using (var conn = rabbitmqConnFactory.CreateConnection())
//{
//    using (IModel channel = conn.CreateModel())
//    {
//        channel.QueueDeclare(queue: "new_sku", durable: true, exclusive: false, autoDelete: false, arguments: null);
//        channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
//        var consumer = new EventingBasicConsumer(channel);

//        var productService = serviceScope.ServiceProvider.GetService(typeof(IProductService)) as IProductService;
//        consumer.Received += (model, args) => // 定义消息消费 handle
//        {
//            var body = args.Body.ToArray();
//            var message = Encoding.UTF8.GetString(body);
//            Log.Logger.Information("准备消费一条新SKU添加消息");
//            var result = productService.AddProduct(JsonConvert.DeserializeObject<AddProductDto>(message)).Result;
//            channel.BasicAck(deliveryTag: args.DeliveryTag, multiple: false); // 确认该消息已经消费完成
//            Log.Logger.Information("一条新SKU添加消息消费完毕");
//        };

//        channel.BasicConsume(queue: "new_sku", autoAck: false, consumer: consumer);
//    }
//}
#endregion
