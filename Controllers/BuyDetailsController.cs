using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS.Models;

namespace POS.Controllers
{
    public class BuyDetailsController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _db;

        public BuyDetailsController(ILogger<HomeController> logger, AppDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public IActionResult BuyDetails(int id)
        {
            var buyDetails = _db.Buy
                .FirstOrDefault(b => b.BuyId == id );
            if (buyDetails == null)
            {
                return NotFound();
            }
            return View(buyDetails);
        }
    }
}
