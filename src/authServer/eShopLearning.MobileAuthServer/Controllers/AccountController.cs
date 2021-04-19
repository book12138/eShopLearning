using Consul;
using eShopLearning.Common;
using eShopLearning.MobileAuthServer.Models;
using IdentityServer4;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Timeout;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace eShopLearning.MobileAuthServer.Controllers
{
    public class AccountController : Controller
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly IEventService _events;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly IConsulClient _consulClient;

        public AccountController(
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IAuthenticationSchemeProvider schemeProvider,
            IEventService events,
            IConfiguration configuration,
            ILogger<AccountController> logger,
            IConsulClient consulClient
            )
        {
            _interaction = interaction;
            _clientStore = clientStore;
            _schemeProvider = schemeProvider;
            _events = events;
            _configuration = configuration;
            _logger = logger;
            _consulClient = consulClient;
        }

        /// <summary>
        /// 注册
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Register(string returnUrl) => View(new RegisterViewModel() { ReturnUrl = returnUrl });
        /// <summary>
        /// 处理注册请求
        /// </summary>
        /// <param name="registerViewModel"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
        {
            if (registerViewModel.Password?.Trim() != registerViewModel.PasswordAgain?.Trim())
            {
                ModelState.AddModelError("", "The two passwords you typed do not match. ");
                return View(registerViewModel);
            }

            var context = await _interaction.GetAuthorizationContextAsync(registerViewModel.ReturnUrl);

            /* consul */
            var userServices = _consulClient.Catalog.Service("microservice_users").Result.Response;
            if (userServices is null || userServices.Any() is false)
            {
                ModelState.AddModelError("", "server error");
                return View(registerViewModel);
            }

            /* 定义超时与熔断策略 */
            ISyncPolicy policy = Polly.Policy.Wrap(
                Polly.Policy.Timeout(10, TimeoutStrategy.Pessimistic),
                Polly.Policy.Handle<Exception>().CircuitBreaker(3, TimeSpan.FromSeconds(3)));

            var responseContent = await policy.Execute(async () =>
            {
                var service = userServices.ElementAt(new Random().Next(userServices.Count()));

                var httpClient = new HttpClient();
                var req = new { username = registerViewModel.Username, password = registerViewModel.Password };
                var encodedContent = new StringContent(JsonConvert.SerializeObject(req));
                encodedContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = await httpClient.PostAsync($"http://{service.ServiceAddress}:{service.ServicePort}/api/Account/Register", encodedContent);
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    throw new Exception("未能正确请求用户服务");
                return response.Content.ReadAsStringAsync().Result;
            });
            ResponseModel<string> userServiceResponse = null;
            if (!string.IsNullOrEmpty(responseContent))
                userServiceResponse = JsonConvert.DeserializeObject<ResponseModel<string>>(responseContent);

            /* 对用户服务返回的结果进行下一步逻辑处理 */
            if (userServiceResponse is not null && userServiceResponse.Code is (int)PublicStatusCode.Success)
            {
                await _events.RaiseAsync(new UserLoginSuccessEvent(registerViewModel.Username, userServiceResponse.Data, registerViewModel.Username, clientId: context?.Client.ClientId));

                var props = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30),
                    AllowRefresh = true,
                    RedirectUri = registerViewModel.ReturnUrl
                };
                var isuser = new IdentityServerUser(userServiceResponse.Data)
                {
                    DisplayName = registerViewModel.Username
                };

                await HttpContext.SignInAsync(isuser, props); // 颁发 token   using Microsoft.AspNetCore.Http;

                if (_interaction.IsValidReturnUrl(registerViewModel.ReturnUrl))
                    return Redirect(registerViewModel.ReturnUrl);

                return RedirectToAction(nameof(Default));
            }

            ModelState.AddModelError("", userServiceResponse.Msg);
            return View(registerViewModel);
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Login(string returnUrl) => View(new LoginViewModel() { ReturnUrl = returnUrl });
        /// <summary>
        /// 处理登录请求
        /// </summary>
        /// <param name="loginViewModel"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
            var context = await _interaction.GetAuthorizationContextAsync(loginViewModel.ReturnUrl);

            /* consul */
            var userServices = _consulClient.Catalog.Service("microservice_users").Result.Response;
            if (userServices is null || userServices.Any() is false)
            {
                ModelState.AddModelError("", "server error");
                return View(loginViewModel);
            }

            /* 定义超时与熔断策略 */
            ISyncPolicy policy = Polly.Policy.Wrap(
                Polly.Policy.Timeout(10, TimeoutStrategy.Pessimistic),
                Polly.Policy.Handle<Exception>().CircuitBreaker(3, TimeSpan.FromSeconds(3)));

            var responseContent = await policy.Execute(async () =>
            {
                var service = userServices.ElementAt(new Random().Next(userServices.Count()));

                var httpClient = new HttpClient();
                var req = new { username = loginViewModel.Username, password = loginViewModel.Password };
                var encodedContent = new StringContent(JsonConvert.SerializeObject(req));
                encodedContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = await httpClient.PostAsync($"http://{service.ServiceAddress}:{service.ServicePort}/api/Account/PasswordCheck", encodedContent);
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    throw new Exception("未能正确请求用户服务");
                return response.Content.ReadAsStringAsync().Result;
            });
            ResponseModel<UserPasswordCheckResultWithUserInfoDto> userServiceResponse = null;
            if (!string.IsNullOrEmpty(responseContent))
                userServiceResponse = JsonConvert.DeserializeObject<ResponseModel<UserPasswordCheckResultWithUserInfoDto>>(responseContent);

            /* 对用户服务返回的结果进行下一步逻辑处理 */
            if (userServiceResponse is not null && userServiceResponse.Data.IsTrue)
            {
                var user = userServiceResponse.Data.UserInfo;
                await _events.RaiseAsync(new UserLoginSuccessEvent(user.Username, user.Id.ToString(), user.NickName, clientId: context?.Client.ClientId));

                var props = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30),
                    AllowRefresh = true,
                    RedirectUri = loginViewModel.ReturnUrl
                };
                var isuser = new IdentityServerUser(user.Id.ToString())
                {
                    DisplayName = user.Username
                };

                await HttpContext.SignInAsync(isuser, props); // 颁发 token   using Microsoft.AspNetCore.Http;

                if (_interaction.IsValidReturnUrl(loginViewModel.ReturnUrl))
                    return Redirect(loginViewModel.ReturnUrl);

                return RedirectToAction(nameof(Default));
            }

            ModelState.AddModelError("", "ERROR Incorrect username or password");
            return View(loginViewModel);
        }

        /// <summary>
        /// 当return url没有时的默认跳转页面
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Default() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
