using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using POS.Models;

namespace POS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult ClientDashboard()
        {
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

        public ActionResult ProductSell()
        {
            return View();
        }

        public ActionResult ProductSellReport()
        {
            return View();
        }

        public ActionResult AddCustomer()
        {
            return View();
        }

        public ActionResult CustomerList()
        {
            return View();
        }

        public ActionResult CustomerDue()
        {
            return View();
        }

        public ActionResult ShopHishab()
        {
            return View();
        }

        public ActionResult ProductBuy()
        {
            return View();
        }

        public ActionResult ProductBuyList()
        {
            return View();
        }

        public ActionResult AddSeller()
        {
            return View();
        }

        public ActionResult SellerList()
        {
            return View();
        }

        public ActionResult AddProduct()
        {
            return View();
        }

        public ActionResult ProductList()
        {
            return View();
        }

        public ActionResult OwnShopDue()
        {
            return View();
        }
        public ActionResult OwnShopHisab()
        {
            return View();
        }
    }
}
