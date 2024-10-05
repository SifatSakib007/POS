using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS.Models;

namespace POS.Controllers
{
    public class RegLogController : Controller
    {

        private readonly AppDbContext _db;

        public RegLogController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: Register - Handle form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Users model)
        {
            if (ModelState.IsValid)
            {
                // Check if password and confirm password match
                if (model.Password != model.ConfirmPassword)
                {
                    ModelState.AddModelError("ConfirmPassword", "Passwords do not match.");
                    return View(model);
                }

                // Add the user to the database
                _db.Users.Add(model);
                await _db.SaveChangesAsync();

                // Redirect to a confirmation page or login page after registration
                return RedirectToAction("Login", "RegLog");
            }

            // If model state is invalid, return the view with validation errors
            return View(model);
        }


        // GET: Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        public IActionResult Login(string phoneNo, string password)
        {
            if (string.IsNullOrEmpty(phoneNo) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "ফোন নম্বর এবং পাসওয়ার্ড আবশ্যক।");
                return View();
            }

            // Check if the user exists with the given phone number and password
            var user = _db.Users.FirstOrDefault(u => u.PhoneNo == phoneNo && u.Password == password);

            if (user == null)
            {
                ModelState.AddModelError("", "ফোন নম্বর বা পাসওয়ার্ড সঠিক নয়।");
                return View();
            }

            // Authentication successful - you can set up session or authentication here
            // Example: HttpContext.Session.SetString("UserId", user.User_Id.ToString());

            return RedirectToAction("Index", "Home"); // Redirect to some home or dashboard page after successful login
        }
    
    
}
}
