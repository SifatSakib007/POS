using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using POS.Models;

namespace POS.Controllers
{
    public class TrackExpenseController : Controller
    {
        private readonly AppDbContext _db;
        public TrackExpenseController(AppDbContext db)
        {
            _db = db;
        }

        public IActionResult CalculateExp()
        {
            return View();
        }
        [HttpPost]
        public IActionResult CalculateExp(Expense model)
        {
            if(ModelState.IsValid)
            {
                _db.Expense.Add(model);
                _db.SaveChanges();
                return RedirectToAction("ClientDashboard", "Home");
            }
            return View();
        }

    }
}
