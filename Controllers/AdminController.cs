using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS.Models;
using System.Linq;

namespace POS.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _db;

        public AdminController(AppDbContext db)
        {
            _db = db;
        }

        public IActionResult AdmClientList()
        {
            // Fetch all active customers
            var customers = _db.Users.Where(u => u.Role == "Client").ToList();
            return View(customers);
        }

        // Handle the deletion of a customer (set status to Inactive)
        [HttpPost]
        public IActionResult DeleteCustomer(int id)
        {
            var customer = _db.Users.Find(id);
            if (customer != null)
            {
                customer.Status = "Inactive"; // Set status to Inactive instead of deleting
                _db.SaveChanges();
            }

            return RedirectToAction("CustomerList");
        }
        [HttpPost]
        public JsonResult ToggleStatus(int id)
        {
            var user = _db.Users.Find(id);
            if (user != null)
            {
                user.Status = user.Status == "Active" ? "Inactive" : "Active";
                _db.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }


    }
}
