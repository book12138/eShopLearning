using ClientDemo.ApplicationServices;
using ClientDemo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ClientDemo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ITestService _testService;

        public HomeController(ILogger<HomeController> logger, ITestService testService)
        {
            _logger = logger;
            _testService = testService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// 尝试请求api网关
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public async Task<IActionResult> TestApiGateway()
        {
            ViewData["data"] = await _testService.TestApiGatewayService();
            return View();
        }

        /// <summary>
        /// 尝试请求api demo
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public async Task<IActionResult> TestApiService()
        {
            ViewData["data"] = await _testService.TestApiService();
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
