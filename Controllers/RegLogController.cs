using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS.Models;
using System.Security.Claims;

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

                // Set default role as "Client"
                model.Role = "Client";  // Assign "Client" as the default role

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

            // Set up claims for authentication
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim(ClaimTypes.Role, user.Role) // Add role claim
    };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                // Options like expiration, persistence, etc.
            };

            // Sign in the user
            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

            // Redirect based on the role
            if (user.Role == "Admin")
            {
                return RedirectToAction("AdminDashboard", "Dashboard");
            }
            else if (user.Role == "Client")
            {
                return RedirectToAction("ClientDashboard", "Dashboard");
            }

            return RedirectToAction("Index", "Home"); // Redirect to some home or dashboard page after successful login
        }
    
    
}
}
