using Consul;
using eShopLearning.Products.Aop;
using eShopLearning.Products.ApplicationServices;
using eShopLearning.Products.ApplicationServices.Impl;
using eShopLearning.Products.AutoMapper;
using eShopLearning.Products.Domain.Bus;
using eShopLearning.Products.Domain.Commands;
using eShopLearning.Products.Domain.Commands.Handlers;
using eShopLearning.Products.Domain.Events;
using eShopLearning.Products.Domain.Events.Handlers;
using eShopLearning.Products.EFCoreRepositories.EFCore;
using eShopLearning.Products.gRPC;
using eShopLearning.Products.gRPC.Protos;
using eShopLearning.Users.EFCoreRepositories.Repositories;
using eShopLearning.Users.EFCoreRepositories.Repositories.Impl;
using HealthChecks.UI.Client;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Nest;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using RabbitMQ.Client;
using Serilog;
using System;
using System.Reflection;

namespace eShopLearning.Products
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Env = env;

            Log.Logger.Information("当前应用环境为：" + Env.EnvironmentName);
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Env { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(o =>
            {
                o.Filters.Add(typeof(GlobalActionFilter));

                if (!Env.IsDevelopment())
                    o.Filters.Add(typeof(GlobalExceptionFilter));
            })
            #region newtonsoft
                .AddNewtonsoftJson(options =>
                {
                    //修改属性名称的序列化方式
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

                    //修改时间的序列化方式
                    options.SerializerSettings.Converters.Add(new IsoDateTimeConverter() { DateTimeFormat = "yyyy/MM/dd HH:mm:ss" });
                }
            );
            #endregion

            #region swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "eShopLearning Product Microservice", Version = "v1" });
            });
            #endregion

            #region ef core
            services.AddDbContext<eShopProductDbContext>(options =>
                     options.UseMySql(Configuration["MysqlConnStr"],
                     ServerVersion.AutoDetect(Configuration["MysqlConnStr"]),
                     mySqlOptionsAction: options =>
                     {
                         options.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
                         options.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                     }));
            #endregion

            #region health check
            services.AddHealthChecks()
                        .AddCheck("self", () => HealthCheckResult.Healthy())
                        .AddRedis(Configuration["RedisConnStr"],
                            name: "eShopLearning.UserService-check",
                            tags: new string[] { "eShopLearning.UserService" });
            #endregion

            #region elasticsearch

            services.AddSingleton<IElasticClient>(options =>
            new ElasticClient(
                new ConnectionSettings(
                    new Uri(Configuration["Elasticsearch"])
                    )
                )
            );

            #endregion

            #region rabbitmq
            services.AddSingleton<IConnectionFactory>(u => new ConnectionFactory()
            {
                HostName = Configuration["RabbitMQ:HostName"],
                UserName = Configuration["RabbitMQ:UserName"],
                Password = Configuration["RabbitMQ:Password"],
                Port = int.TryParse(Configuration["RabbitMQ:Port"], out int parseResult) ? parseResult : 5672
            });
            #endregion

            #region domain            
            services.AddMediatR(typeof(Startup)); // MediatR
            services.AddScoped<IApplicationBus, ApplicationBus>(); // bus            
            services.AddScoped<IRequestHandler<AddProductCommand, Unit>, ProductCommandHandler>(); // 领域命令         
            services.AddScoped<INotificationHandler<DomainNotification>, DomainNotificationHandler>(); // 领域通知
            services.AddScoped<INotificationHandler<AddProductEvent>, ProductEventHandler>(); // 领域事件
            #endregion

            services.AddAutoMapper(typeof(CustomProfile)); // automapper
            services.AddGrpc(); // gRPC

            #region application repositories
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ISkuRepository, SkuRepository>();
            services.AddScoped<ISpuRepository, SpuRepository>();
            services.AddScoped<ISkuAttrRepository, SkuAttrRepository>();
            #endregion

            #region application services 
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ISkuEsService, SkuEsService>();
            #endregion            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // 服务注册
            var consulClient = new ConsulClient(u => u.Address = new Uri(Configuration["ConsulAddress"]));
            consulClient.Agent.ServiceRegister(new AgentServiceRegistration()
            {
                ID = Guid.NewGuid().ToString(),
                Name = "microservice_product",// 服务名
                Address = "localhost", // 服务绑定IP
                Port = 7648, // 服务绑定端口
                Check = new AgentServiceCheck()
                {
                    DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(5),//服务启动多久后注册
                    Interval = TimeSpan.FromSeconds(10),//健康检查时间间隔
                    HTTP = $"http://localhost:7648/api/Health/Check",//健康检查地址
                    Timeout = TimeSpan.FromSeconds(5)
                }
            }).Wait();
            consulClient.Agent.ServiceRegister(new AgentServiceRegistration()
            {
                ID = Guid.NewGuid().ToString(),
                Name = "microservice_product_gRPC",// 服务名
                Address = "localhost", // 服务绑定IP
                Port = 8685, // 服务绑定端口
                Check = new AgentServiceCheck()
                {
                    DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(5),//服务启动多久后注册
                    Interval = TimeSpan.FromSeconds(10),//健康检查时间间隔
                    HTTP = $"http://localhost:8685/Check",//健康检查地址
                    Timeout = TimeSpan.FromSeconds(5)
                }
            }).Wait();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "eShopLearning.Products v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                /* health check */
                endpoints.MapHealthChecks("/hc", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
                endpoints.MapHealthChecks("/liveness", new HealthCheckOptions
                {
                    Predicate = r => r.Name.Contains("self")
                });
                /* gRPC */
                endpoints.MapGrpcService<SkuInfoGrpcService>();
                endpoints.MapGrpcService<HealthCheckGrpcService>();
            });
        } 
    }
}
