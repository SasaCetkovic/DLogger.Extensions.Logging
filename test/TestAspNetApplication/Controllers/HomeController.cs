using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace TestAspNetApplication.Controllers
{
    public class HomeController : Controller
    {
		private ILogger _logger;

		public HomeController(ILogger<HomeController> logger)
		{
			_logger = logger;
		}

        public IActionResult Index()
        {
			_logger.LogInformation("Inside Index()");

			using (_logger.BeginScope("Index scope"))
			{
				_logger.LogInformation("first scope level");

				using (_logger.BeginScope("second index scope"))
				{
					_logger.LogInformation("second scope level");
				}
			}

			return View();
        }

        public IActionResult About()
        {
			// Test bulk write feature
			for (int i = 0; i < 1000; i++)
			{
				_logger.LogDebug("Inside About()");
			}

			ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
			_logger.LogWarning("Inside Contact()");
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
			_logger.LogError("Inside Error()", new Exception("Some error message"));
            return View();
        }
    }
}
