using Microsoft.AspNetCore.Mvc;
using ScrappingManagement.Web.Models;
using System.Diagnostics;
using ScrappingManagement.Web.Services;
namespace ScrappingManagement.Web.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly WhatsAppInvoiceService _service;

          public HomeController(ILogger<HomeController> logger, WhatsAppInvoiceService service)
		{
			_logger = logger;
               _service = service;

          }

		public IActionResult Index()
		{
			return View();
		}
          public IActionResult Send()
          {
               _service.SendInvoiceTextAsync("whatsapp:+917623830205", "INV-1001", 250.75m, "2024-07-15").Wait();
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
