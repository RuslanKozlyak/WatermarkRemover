using InpainterWebapp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace InpainterWebapp.Controllers
{
    public class HomeController : Controller
    {

        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;

        }

        
        private static async Task InvokeAsync(object[] n)
        {
            using var rpcClient = new RpcClient();

            Console.WriteLine(" [x] Requesting ({0})", String.Join(" ", n[0]));
            var response = await rpcClient.CallAsync(n);
            Console.WriteLine(" [.] Got '{0}'", response);
        }

        // Тестовый запрос из документации Celery, складывает два числа на стороне питона
        // и возвращает ответ в C#
        public async Task<IActionResult> Index()
        {
            object[] taskArgs = new object[] { 1, 200 };

            object[] arguments = new object[] { taskArgs, new object(), new object() };
            await InvokeAsync(arguments);
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
