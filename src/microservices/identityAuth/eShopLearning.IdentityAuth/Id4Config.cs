using AutoMapper.Configuration;
using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;

namespace IdentityServer4Demo
{
    /// <summary>
    /// identity server 4 的一些必要的配置数据
    /// </summary>
    public class Id4Config
    {
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email(),
            };
        }

        /// <summary>
        /// 加载 api scope
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<ApiScope> GetApiScopes()
        {
            return new List<ApiScope>
            {
                // demo api
                new ApiScope("demoapi"),
                new ApiScope("demoapigateway"),
                // eshop http api
                new ApiScope("eshophttpapigateway"),
                new ApiScope("productapi"),
                new ApiScope("userapi"),
                new ApiScope("cartapi"),
            };
        }

        /// <summary>
        /// 加载 api resource
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<ApiResource> GetResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("demos", "Demo API")
                {
                    ApiSecrets = { new Secret("secret".Sha256()) },

                    Scopes = { "demoapi", "demoapigateway" }
                },
                new ApiResource("eshopApis", "eShop apis")
                {
                    ApiSecrets = { new Secret("secret".Sha256()) },
                    Scopes = { "eshophttpapigateway" , "productapi", "userapi", "cartapi" }
                }
            };
        }

        /// <summary>
        /// 加载客户端
        /// </summary>
        /// <param name="clientUrls">客户端url集合</param>
        /// <returns></returns>
        public static IEnumerable<Client> GetClients(IDictionary<string, string> clientUrls)
        {
            return new List<Client>
            {
                // mvc client demo
                new Client
                {
                    ClientId = "mvcclientdemo",
                    ClientName = "Interactive client (Code with PKCE)",
                    ClientSecrets = { new Secret("NBJUju*&#%5sxajYGV21Jsab__JJsa1213$566sabHHHsja".Sha256()) },
                    AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
                    AllowAccessTokensViaBrowser = true,
                    RequireConsent = false, // 是否在登录成功的时候，进入许可页面
                    AllowOfflineAccess = true, // 是否允许离线访问
                    RedirectUris = { $"{clientUrls["mvcclientdemo"]}/signin-oidc" }, // 登录成功后跳转的地址
                    PostLogoutRedirectUris = { $"{clientUrls["mvcclientdemo"]}/signout-callback-oidc" }, // 登出时的跳转地址
                    AllowedScopes = {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.OfflineAccess,
                        "demoapi",
                        "demoapigateway" // 定义该客户端可使用的 api 资源
                    },
                    AccessTokenLifetime = 60*60*2, // 2 hours
                    IdentityTokenLifetime= 60*60*2, // 2 hours
                    RefreshTokenUsage = TokenUsage.ReUse,
                    RefreshTokenExpiration = TokenExpiration.Sliding, // token 为滑动过期时间
                    RequirePkce = true
                },
                // eshop uniapp
                new Client
                {
                    ClientId = "eshopuniapp",
                    ClientName = "eShop Uniapp",
                    ClientSecrets = { new Secret("UoibcsialUC&A*ougscila^A&Ics12gyd*OIygysa".Sha256()) },
                    AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
                    AllowAccessTokensViaBrowser = true,
                    RequireConsent = false, // 是否在登录成功的时候，进入许可页面
                    AllowOfflineAccess = true, // 是否允许离线访问
                    RedirectUris = { $"{clientUrls["eshopuniapp"]}/signin-oidc" }, // 登录成功后跳转的地址
                    PostLogoutRedirectUris = { $"{clientUrls["eshopuniapp"]}/signout-callback-oidc" }, // 登出时的跳转地址
                    AllowedScopes = {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.OfflineAccess,
                        "eshophttpapigateway",
                        "productapi",
                        "userapi",
                        "cartapi"
                    },
                    AccessTokenLifetime = 60*60*2, // 2 hours
                    IdentityTokenLifetime= 60*60*2, // 2 hours
                    RefreshTokenUsage = TokenUsage.ReUse,
                    RefreshTokenExpiration = TokenExpiration.Sliding, // token 为滑动过期时间
                    RequirePkce = true
                },
                // eShopHttpAggregator Swagger UI
                new Client
                {
                    ClientId = "eshophttpaggswaggerui",
                    ClientName = "eShop http Aggregattor Swagger UI",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,
                    RedirectUris = { $"{clientUrls["eshophttpaggswaggerui"]}/swagger/oauth2-redirect.html" },
                    PostLogoutRedirectUris = { $"{clientUrls["eshophttpaggswaggerui"]}/swagger/" },
                    AllowedScopes = {
                        "eshophttpapigateway",
                        "productapi",
                        "userapi",
                        "cartapi"
                    }
                },
            };
        }
    }
}
