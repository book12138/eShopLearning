using eShopLearning.Users.Aop;
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
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using NConsul.AspNetCore;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

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
            services.AddSwaggerGen(c =>
                   {
                       c.SwaggerDoc("v1", new OpenApiInfo { Title = "eShopLearning User Microservice", Version = "v1" });
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

            #region health check
            services.AddHealthChecks()
                        .AddCheck("self", () => HealthCheckResult.Healthy())
                        .AddRedis(Configuration["RedisConnStr"],
                            name: "eShopLearning.UserService-check",
                            tags: new string[] { "eShopLearning.UserService" });
            #endregion

            #region consul
            services.AddConsul(Configuration["ConsulAddress"])
           .AddHttpHealthCheck("http://localhost:7648/api/Health/Check", 5, 10)
           .RegisterService("microservice_users", "localhost", 1685, new string[0]);
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
            if (Env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "eShopLearning User Microservice v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/hc", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
                endpoints.MapHealthChecks("/liveness", new HealthCheckOptions
                {
                    Predicate = r => r.Name.Contains("self")
                });
            });
        }
    }
}
