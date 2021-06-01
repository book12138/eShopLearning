using eShopLearning.Common.Extension.AspNetCoreFilter;
using eShopLearning.Products.ApplicationServices;
using eShopLearning.Products.ApplicationServices.Impl;
using eShopLearning.Products.AutoMapper;
using eShopLearning.Products.Domain.Bus;
using eShopLearning.Products.Domain.Commands;
using eShopLearning.Products.Domain.Commands.Handlers;
using eShopLearning.Products.Domain.Events;
using eShopLearning.Products.Domain.Events.Handlers;
using eShopLearning.Products.EFCoreRepositories.EFCore;
using eShopLearning.Products.ApplicationGrpcRemoteServices;
using eShopLearning.Users.EFCoreRepositories.Repositories;
using eShopLearning.Users.EFCoreRepositories.Repositories.Impl;
using HealthChecks.UI.Client;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NConsul.AspNetCore;
using Nest;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using RabbitMQ.Client;
using Serilog;
using System;
using System.Collections.Generic;
using System.Reflection;
using eShopLearning.Products.ApplicationServices.QueryServices;
using eShopLearning.Products.ApplicationServices.QueryServices.Impl;

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
            services.AddScoped<IRequestHandler<SkuInfoPersistentToEsCommand, Unit>, ProductCommandHandler>();
            services.AddScoped<INotificationHandler<DomainNotification>, DomainNotificationHandler>(); // 领域通知
            services.AddScoped<INotificationHandler<AddProductEvent>, ProductEventHandler>(); // 领域事件
            #endregion

            services.AddAutoMapper(typeof(CustomProfile)); // automapper
            services.AddGrpc(); // gRPC

            #region consul

            /* 从 apollo 配置中心里获取 ip 及 端口 的配置 */
            var webapiServiceEndpointIp = Configuration["WebApiEndPointIp"] ?? "localhost";
            var webapiServiceEndpointPort = Convert.ToInt32(Configuration["WebApiEndPointPort"] ?? "7648");
            var grpcServiceEndpointIp = Configuration["GrpcEndPointIp"] ?? "localhost";
            var grpcServiceEndpointPort = Convert.ToInt32(Configuration["GrpcEndPointPort"] ?? "8685");

            /* 注册到consul中 */
            services.AddConsul(Configuration["ConsulAddress"])
                .AddHttpHealthCheck($"http://{webapiServiceEndpointIp}:{webapiServiceEndpointPort}/api/Health/Check", 5, 10)
                .RegisterService("microservice_product", webapiServiceEndpointIp, webapiServiceEndpointPort, new string[0]);
            services.AddConsul(Configuration["ConsulAddress"])
                .AddGRPCHealthCheck($"{grpcServiceEndpointIp}:{grpcServiceEndpointPort}")
                .RegisterService("microservice_product_grpc", grpcServiceEndpointIp, grpcServiceEndpointPort, new string[0]);
            #endregion

            #region swagger
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("productApiDoc", new OpenApiInfo
                {
                    Title = "eShop Product Service",
                    Version = "v1",
                    Description = "eShop Product Api Swagger UI"
                });

                options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows()
                    {
                        Implicit = new OpenApiOAuthFlow()
                        {
                            AuthorizationUrl = new Uri($"{Configuration.GetValue<string>("IdentityAuthServerUrl")}/connect/authorize"),
                            TokenUrl = new Uri($"{Configuration.GetValue<string>("IdentityAuthServerUrl")}/connect/token"),
                            Scopes = new Dictionary<string, string>()
                             {
                                 { "productapi", "product service api" }
                             }
                        }
                    }
                });

                options.OperationFilter<AuthorizeCheckOperationFilter>();
            });
            #endregion

            #region authentication
            var identityUrl = Configuration.GetValue<string>("IdentityAuthServerUrl");
            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    // 指定要接入的授权服务器地址
                    options.Authority = identityUrl;
                    // 在验证token时，不验证Audience
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = false
                    };
                    // 不使用Https
                    options.RequireHttpsMetadata = false;
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("eshopApiScop", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("scope", "productapi");
                });
            });
            #endregion

            #region application repositories
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ISkuRepository, SkuRepository>();
            services.AddScoped<ISpuRepository, SpuRepository>();
            services.AddScoped<ISkuAttrRepository, SkuAttrRepository>();
            #endregion

            #region application services 
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ISkuEsService, SkuEsService>();
            services.AddScoped<IProductQueryService, ProductQueryService>();
            #endregion            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var pathBase = Configuration["PATH_BASE"];
            if (!string.IsNullOrEmpty(pathBase))
            {
                Log.Information("当前应用运行路径为 '{pathBase}'", pathBase);
                app.UsePathBase(pathBase);
            }

            if (Env.IsDevelopment() || Env.IsStaging())
            {
                app.UseSwagger().UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint($"{ (!string.IsNullOrEmpty(pathBase) ? pathBase : string.Empty) }/swagger/productApiDoc/swagger.json", "Purchase BFF V1");

                    c.OAuthClientId("productserviceswaggerui");
                    c.OAuthClientSecret(string.Empty);
                    c.OAuthRealm(string.Empty);
                    c.OAuthAppName("eShop Product Api Swagger UI");
                });

                app.UseViewConfig(u => u.RenderPage());
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                /* gRPC */
                endpoints.MapGrpcService<SkuInfoGrpcService>();
                endpoints.MapGrpcService<HealthCheckGrpcService>();
            });
        } 
    }
}
