using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace POS.Controllers
{
    public class ClientEmpController : Controller
    {
        private readonly AppDbContext _db;

        public ClientEmpController(AppDbContext db)
        {
            _db = db;
        }

        private int? GetLoggedInUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdString, out int userId))
            {
                return userId;
            }
            return null;
        }

        // Display all employees and the form to add a new employee
        [HttpGet]
        public async Task<IActionResult> AddEmp()
        {
            // Get the logged-in user's ID
            int? loggedInUserId = GetLoggedInUserId();

            // Fetch the logged-in user to get their EmployeeId string
            var loggedInUser = await _db.Users.SingleOrDefaultAsync(u => u.UserId == loggedInUserId);

            // Check if the logged-in user was found and has an EmployeeId
            if (loggedInUser != null && !string.IsNullOrEmpty(loggedInUser.EmployeeId))
            {
                // Split the EmployeeId string into a list of IDs
                var employeeIds = loggedInUser.EmployeeId.Split(',').Select(id => id.Trim()).ToList();

                // Fetch employees whose UserId matches the IDs from the logged-in user's EmployeeId
                var employees = await _db.Users
                    .Where(s => employeeIds.Contains(s.UserId.ToString())) // Match UserId with EmployeeId
                    .ToListAsync();

                var viewModel = new EmployeeViewModel
                {
                    NewEmployee = new Users
                    {
                        UserName = string.Empty, // Set default values
                        ShopName = string.Empty,
                        Address = string.Empty,
                        PhoneNo = string.Empty,
                        Password = string.Empty,
                        ConfirmPassword = string.Empty
                    }, // Create a new Users instance for the form
                    EmployeeList = employees // Assign the filtered employees to the EmployeeList
                };

                return View(viewModel); // Pass the view model to the view
            }

            // If no matching employees are found or logged-in user has no EmployeeId, return an empty list
            var emptyViewModel = new EmployeeViewModel
            {
                NewEmployee = new Users
                {
                    UserName = string.Empty, // Set default values
                    ShopName = string.Empty,
                    Address = string.Empty,
                    PhoneNo = string.Empty,
                    Password = string.Empty,
                    ConfirmPassword = string.Empty
                },
                EmployeeList = new List<Users>()
            };

            return View(emptyViewModel); // Return an empty view model
        }


        // Handle form submission to create a new employee
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEmp(EmployeeViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var transaction = await _db.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // Step 1: Ensure the new employee object is properly initialized
                        if (model.NewEmployee != null)
                        {
                            model.NewEmployee.CreatedAt = DateTime.Now; // Set CreatedAt to now

                            // Add the new employee to the context
                            _db.Users.Add(model.NewEmployee);
                            await _db.SaveChangesAsync(); // Save changes to the database

                            // Step 2: Update the existing user's EmployeeId with the new user's ID
                            // Assuming you want to update a specific existing user
                            int? existingUserId = GetLoggedInUserId(); // Replace with the actual logic to get the user ID
                            var existingUser = await _db.Users.SingleOrDefaultAsync(u => u.UserId == existingUserId);
                            if (existingUser != null)
                            {
                                // Step 3: Update the EmployeeId to include the new user's ID
                                if (string.IsNullOrEmpty(existingUser.EmployeeId))
                                {
                                    existingUser.EmployeeId = model.NewEmployee.UserId.ToString(); // First ID being added
                                }
                                else
                                {
                                    existingUser.EmployeeId += $",{model.NewEmployee.UserId}"; // Append new ID
                                }
                                _db.Users.Update(existingUser);
                                await _db.SaveChangesAsync(); // Save changes to the database
                            }
                            else
                            {
                                throw new Exception("Existing user not found");
                            }

                            // Commit the transaction
                            await transaction.CommitAsync();
                            return RedirectToAction(nameof(AddEmp)); // Redirect to the same action to show the updated list
                        }
                    }
                    catch (Exception ex)
                    {
                        // Rollback the transaction if any error occurs
                        await transaction.RollbackAsync();
                        ModelState.AddModelError("", "Error occurred: " + ex.Message);
                    }
                }
                // If model is invalid or NewEmployee is null, retrieve employees again to show in the view
                model.EmployeeList = await _db.Users.ToListAsync(); // Get the updated employee list
                return View(model); // Re-render the page with the model containing errors
            }

            // If model is invalid or NewEmployee is null, retrieve employees again to show in the view
            model.EmployeeList = await _db.Users.ToListAsync(); // Get the updated employee list
            return View(model); // Re-render the page with the model containing errors
        }


        public IActionResult EmpDetails(int id)
        {
            var employee = _db.Users.Find(id);
            if(employee == null)
            {
                return NotFound();
            }
            return View(employee);
        }




    }
}
