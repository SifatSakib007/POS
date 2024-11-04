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
        // GET: Users/Edit/5
        [HttpGet]
        public async Task<IActionResult> EditClient(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(); // Handle the case when user not found
            }
            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditClient(Users user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _db.Update(user); // Update the user entity
                    await _db.SaveChangesAsync(); // Save changes to the database
                    TempData["success"] = "User updated successfully."; // Add success message
                    return RedirectToAction(nameof(Index)); // Redirect to a list or index page
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.UserId))
                    {
                        return NotFound(); // Handle the case when user not found during update
                    }
                    else
                    {
                        throw; // Rethrow the exception if another issue occurs
                    }
                }
            }
            return View(user); // Return to the view with the user data if model is invalid
        }

        private bool UserExists(int id)
        {
            return _db.Users.Any(e => e.UserId == id); // Check if user exists
        }

    }
}
