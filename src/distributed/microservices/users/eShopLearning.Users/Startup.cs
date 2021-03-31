using eShopLearning.Common.Extension.AspNetCoreFilter;
using eShopLearning.Users.ApplicationServices;
using eShopLearning.Users.ApplicationServices.Impl;
using eShopLearning.Users.AutoMapper;
using eShopLearning.Users.EFCoreRepositories.EFCore;
using eShopLearning.Users.EFCoreRepositories.Repositories;
using eShopLearning.Users.EFCoreRepositories.Repositories.Impl;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NConsul.AspNetCore;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Serilog;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace eShopLearning.Users
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
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("userApiDoc", new OpenApiInfo
                {
                    Title = "eShop User Service",
                    Version = "v1",
                    Description = "eShop User Api Swagger UI"
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
                                 { "userapi", "user service api" }
                             }
                        }
                    }
                });

                options.OperationFilter<AuthorizeCheckOperationFilter>();
            });
            #endregion

            #region ef core
                services.AddDbContext<ApplicationUserDbContext>(options =>
                     options.UseMySql(Configuration["MysqlConnStr"],
                     ServerVersion.AutoDetect(Configuration["MysqlConnStr"]),
                     mySqlOptionsAction: options =>
                     {
                         options.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
                         options.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                     }));
            #endregion            

            #region consul
            services.AddConsul(Configuration["ConsulAddress"])
           .AddHttpHealthCheck("http://localhost:1685/api/Health/Check", 5, 10)
           .RegisterService("microservice_users", "localhost", 1685, new string[0]);
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
                    policy.RequireClaim("scope", "userapi");
                });
            });
            #endregion

            services.AddAutoMapper(typeof(CustomProfile)); // automapper

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IBCryptService, BCryptService>();

            services.AddScoped<IIdentityService, IdentityService>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            var pathBase = Configuration["PATH_BASE"];
            if (!string.IsNullOrEmpty(pathBase))
            {
                Log.Information("当前应用运行路径为 '{pathBase}'", pathBase);
                app.UsePathBase(pathBase);
            }

            if(Env.IsDevelopment() || Env.IsStaging())
            {
                app.UseSwagger().UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint($"{ (!string.IsNullOrEmpty(pathBase) ? pathBase : string.Empty) }/swagger/userApiDoc/swagger.json", "Purchase BFF V1");

                    c.OAuthClientId("userserviceswaggerui");
                    c.OAuthClientSecret(string.Empty);
                    c.OAuthRealm(string.Empty);
                    c.OAuthAppName("eShop User Api Swagger UI");
                });

                app.UseViewConfig(x => x.RenderPage());
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
