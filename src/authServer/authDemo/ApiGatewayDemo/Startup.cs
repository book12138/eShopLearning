using ApiGatewayDemo.ApplicationServices;
using ApiGatewayDemo.ApplicationServices.Impl;
using ApiGatewayDemo.Infrastructure.HttpMessageHandler;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace ApiGatewayDemo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ApiGatewayDemo", Version = "v1" });
            });

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
                    // 不适用Https
                    options.RequireHttpsMetadata = false;
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("orderScope", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("scope", "demoapigateway");
                });
            });

            services.AddTransient<HttpClientAuthorizationDelegatingHandler>();
            services.AddHttpClient<IApiTestService, ApiTestService>()
              .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ApiGatewayDemo v1"));
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
