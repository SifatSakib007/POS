using System.Diagnostics;
using System.Globalization;
using System.IO.Pipelines;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using POS.Models;

namespace POS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _db;
        public HomeController(ILogger<HomeController> logger, AppDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        [Authorize(Roles = "Admin")]
        public IActionResult AdminDashboard()
        {
            return View();
        }

        [Authorize(Roles = "Client")]
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

        // GET: ViewModelForProductCustomer - Loads products and customers for selection
        public async Task<IActionResult> ViewModelForProductCustomer()
        {
            var products = await _db.Product.ToListAsync();
            var customers = await _db.Customer.ToListAsync();

            var viewModel = new ProductCustomerViewModel
            {
                Products = products,
                Customers = customers,
                Sells = new Sell
                {
                    ProductName = string.Empty,
                    Quantity = 0,
                    TotalPrice = 0m,
                    TotalTotalPrice = 0m,
                    ShabekDue = 0,
                }
            };

            return View(viewModel);
        }

        // GET: Fetches product details for a specific product ID (for AJAX)
        [HttpGet]
        public async Task<IActionResult> GetProductDetails(int productId)
        {
            var product = await _db.Product.FirstOrDefaultAsync(p => p.ProductId == productId);
            if (product != null)
            {
                return Json(new
                {
                    success = true,
                    data = new
                    {
                        ProductId = product.ProductId,
                        ProductName = product.ProductName,
                        BuyPrice = product.BuyPrice,
                        Stock = product.Stock
                    }
                });
            }
            return Json(new { success = false, message = "Product not found." });
        }

        // AJAX call to fetch customer details
        [HttpGet]
        public async Task<JsonResult> GetCustomerDetails(int customerId)
        {
            var customer = await _db.Customer.FirstOrDefaultAsync(c => c.Id == customerId);
            if (customer != null) {
                return Json(new
                {
                    success = true,
                    data = new
                    {
                        CustomerId = customer.Id,
                        CustomerName = customer.Name,
                        Address = customer.Address,
                        PhoneNo = customer.PhoneNo,
                        Due = customer.Due
                    }
                });
            }
           

            return Json(new { success = false, message = "Customer not found!." });
        }
        // GET: ProductSell - Displays the sell page
        public async Task<IActionResult> ProductSell()
        {
            // Load products and customers for selection in the view
            var products = await _db.Product.ToListAsync();
            var customers = await _db.Customer.ToListAsync();

            // Create the ViewModel and populate it
            var viewModel = new ProductCustomerViewModel
            {
                Products = products,
                Customers = customers,
                Sells = new Sell
                {
                    // Initialize required members
                    ProductName = string.Empty,
                    Quantity = 0,
                    TotalPrice = 0m,
                    TotalTotalPrice = 0m,
                    ShabekDue = 0 // Initialize TotalTotalPrice
                }
            };

            return View(viewModel);
        }

        // POST: ProductSell - Handles the sale transaction
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProductSell(ProductCustomerViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                // Reload the products and customers if validation fails
                viewModel.Products = await _db.Product.ToListAsync();
                viewModel.Customers = await _db.Customer.ToListAsync();
                TempData["error"] = "Something went wrong.";
                return View(viewModel);
            }

            // Fetch the selected product
            var product = await _db.Product.FirstOrDefaultAsync(p => p.ProductId == viewModel.SelectedProductId);
            if (product == null)
            {
                ModelState.AddModelError("", "Invalid product selected.");
                return View(viewModel);
            }

            // Update Sell model with product details
            var sell = viewModel.Sells;
            sell.ProductName = product.ProductName;
            sell.BuyPrice = product.BuyPrice;

            // Ensure Quantity is set before calculating total price
            if (sell.Quantity <= 0)
            {
                ModelState.AddModelError("", "Invalid quantity.");
                return View(viewModel);
            }

            sell.TotalPrice = sell.Quantity * sell.SellingPrice;

            // Update stock after selling
            product.Stock -= sell.Quantity;
            if (product.Stock < 0)
            {
                ModelState.AddModelError("", "Insufficient stock for this product.");
                return View(viewModel);
            }

            // Fetch customer information
            var customer = await _db.Customer.FirstOrDefaultAsync(c => c.Id == viewModel.SelectedCustomerId);
            if (customer == null)
            {
                ModelState.AddModelError("", "Invalid customer selected.");
                return View(viewModel);
            }

            // Update customer info in Sell model
            sell.CustomerName = customer.Name;
            sell.CustomerPhoneNo = customer.PhoneNo;
            sell.CustomerAddress = customer.Address;
            sell.DuePrice = customer.Due;

            // Calculate total due after this transaction
            sell.TotalDuePrice = sell.TotalPrice - sell.Deposit;

            // Save Sell transaction to the database
            _db.Sell.Add(sell);
            await _db.SaveChangesAsync();

            // Update customer's due amount
            customer.Due += sell.TotalDuePrice;
            await _db.SaveChangesAsync();

            TempData["success"] = "Successfully sold!";
            return RedirectToAction("ProductSell");
        }


        public async Task<IActionResult> ProductSellReport()
        {
            // Fetch all the sell records along with related product and customer details
            var sellReport = await _db.Sell
                .Include(s => s.Product) // Include related Product data
                .Include(s => s.Customer) // Include related Customer data
                .ToListAsync();

            // Pass the fetched data to the view
            return View(sellReport);
        }


        // GET: AddCustomer - Display the form for adding a new customer
        public IActionResult AddCustomer()
        {
            return View();
        }

        // POST: AddCustomer - Handle form submission to add a new customer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCustomer(Customer model)
        {
            // Check if the model is valid according to data annotations
            if (ModelState.IsValid)
            {
                // Set the created date to the current time (in case it's not set)
                //model.CreatedAt = DateTime.UtcNow;

                // Add the customer data to the database
                _db.Customer.Add(model);
                await _db.SaveChangesAsync();
                //success toaster alert
                TempData["success"] = "Successfullly added customer details!.";
                // Redirect to a confirmation page or customer list after successful addition
                return RedirectToAction("CustomerList");  // Assuming you have a CustomerList page
            }
            //success toaster alert
            TempData["error"] = "Error saving customer details!.";
            // If the model state is invalid, return the view with validation errors
            return View(model);
        }
        // GET: CustomerList - Show all customers in a table
        public async Task<IActionResult> CustomerList()
        {
            // Fetch all customers from the database
            var customers = await _db.Customer.ToListAsync();

            // Pass the customers to the view
            return View(customers);
        }

        // GET: CustomerDue (Displays the dropdown with all customers)
        public IActionResult CustomerDue()
        {
            // Fetch all customers to populate the dropdown
            var customers = _db.Customer.ToList();
            ViewBag.CustomerList = new SelectList(customers, "Id", "Name"); // Passing customers to dropdown
            return View();
        }

        // GET: Get customer details by ID using AJAX
        [HttpGet]
        public JsonResult GetCustomerDetailsForDueCustomer(int id)
        {
            var customer = _db.Customer.FirstOrDefault(c => c.Id == id);
            if (customer == null)
            {
                return Json(new { success = false, message = "Customer not found" });
            }

            // Return customer details
            return Json(new
            {
                success = true,
                phoneNo = customer.PhoneNo,
                due = customer.Due
        });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CustomerDue(Customer model)
        {
            var customer = _db.Customer.FirstOrDefault(c => c.Id == model.Id);
            if (customer == null)
            {
                return NotFound();
            }

            // Validate the DueClose field
            if (model.DueClose == null || model.DueClose <= 0)
            {
                ModelState.AddModelError(nameof(model.DueClose), "Due Close amount must be greater than 0.");
            }

            /*// Check if ModelState is valid
            if (!ModelState.IsValid)
            {
                
                // Log ModelState errors for debugging
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    
                    Console.WriteLine(error.ErrorMessage); // Log the error
                }

                // Reload the dropdown in case of validation failure
                var customers = _db.Customer.ToList();
                ViewBag.CustomerList = new SelectList(customers, "Id", "Name");
                return View(model); // Return the same view with the model and errors
            }*/

            // Proceed with updating the customer details if ModelState is valid
            customer.Due -= model.DueClose.Value; // Ensure DueClose is not null with .Value
            customer.DueClose = model.DueClose;

            // Your existing code for updating PaymentDates and PaymentAmounts
            // Get the current payment date and amount
            string currentDate = DateTime.UtcNow.ToString("yyyy-MM-dd"); // Format the date
            string currentAmount = model.DueClose.Value.ToString(); // Convert the paid amount to string

            // Append the current date to the PaymentDates column
            customer.PaymentDates = string.IsNullOrWhiteSpace(customer.PaymentDates)
                ? currentDate
                : customer.PaymentDates + "," + currentDate;

            // Append the paid amount to the PaymentAmounts column
            customer.PaymentAmounts = string.IsNullOrWhiteSpace(customer.PaymentAmounts)
                ? currentAmount
                : customer.PaymentAmounts + "," + currentAmount;

            // Save changes to the database
            _db.Customer.Update(customer);
            try
            {
                _db.SaveChanges();
                TempData["success"] = "Successfully updated customer due details!";
                return RedirectToAction("CustomerDue");
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while updating the database!";
                ModelState.AddModelError("", "An error occurred while updating the database");
                // Reload the dropdown
                var customersList = _db.Customer.ToList();
                ViewBag.CustomerList = new SelectList(customersList, "Id", "Name");
                return View(model); // Return the view with errors
            }
        }


        public ActionResult ShopHishab()
        {
            return View();
        }

        public ActionResult ProductBuy()
        {

            return View();
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult ProductBuy(Product product)
        {
            if (ModelState.IsValid)
            {
                //product.CreatedAt = DateTime.UtcNow;
                _db.Product.Add(product);
                _db.SaveChanges();
                //success toaster alert
                TempData["success"] = "Successfullly added product details!.";
                return View(); // This stays on the same page
            }
            //success toaster alert
            TempData["success"] = "Error adding product details!.";
            // If model state is not valid, return the form with validation errors
            return View(product);

        }

        public ActionResult ProductBuyList()
        {
            var products = _db.Product.ToList();
            return View(products);
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
