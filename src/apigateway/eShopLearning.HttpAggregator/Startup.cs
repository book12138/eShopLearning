using eShopLearning.Common.HttpServices.HttpMessageHandler;
using eShopLearning.HttpAggregator.Aop;
using eShopLearning.HttpAggregator.ApplicationServices;
using eShopLearning.HttpAggregator.ApplicationServices.Impl;
using eShopLearning.Products.AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopLearning.HttpAggregator
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
                });
            #endregion

            services.AddOptions();

            #region swagger
            services.AddSwaggerGen(options =>
            {
                options.DescribeAllEnumsAsStrings();

                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Shopping Aggregator for Uniapp Clients",
                    Version = "v1",
                    Description = "Shopping Aggregator for Uniapp Clients"
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
                                { "webshoppingagg", "Shopping Aggregator for Uniapp Clients" }
                            }
                        }
                    }
                });

                options.OperationFilter<AuthorizeCheckOperationFilter>();
            });
            #endregion

            services.AddAutoMapper(typeof(CustomProfile)); // automapper

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
                    policy.RequireClaim("scope", "eshophttpapigateway");
                });
            });
            #endregion

            #region http services
            services.AddTransient<HttpClientAuthorizationDelegatingHandler>();
            services.AddHttpClient<ICartService, CartService>()
              .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
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

            app.UseSwagger().UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"{ (!string.IsNullOrEmpty(pathBase) ? pathBase : string.Empty) }/swagger/v1/swagger.json", "Purchase BFF V1");

                c.OAuthClientId("webshoppingaggswaggerui");
                c.OAuthClientSecret(string.Empty);
                c.OAuthRealm(string.Empty);
                c.OAuthAppName("web shopping bff Swagger UI");
            });

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
