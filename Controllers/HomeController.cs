using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
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



        // GET: ProductSell
        public async Task<IActionResult> ProductSell()
        {
            // Load products and customers for selection in the view
            var products = await _db.Product.ToListAsync();
            var customers = await _db.Customer.ToListAsync();

            // Return the view with data
            ViewBag.Products = products;
            ViewBag.Customers = customers;

            return View();
        }

        // POST: ProductSell - Handles the sale transaction
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProductSell(Sell model)
        {
            if (!ModelState.IsValid)
            {
                // Reload the necessary data if validation fails
                ViewBag.Products = await _db.Product.ToListAsync();
                ViewBag.Customers = await _db.Customer.ToListAsync();
                return View(model);
            }

            // Fetch the selected product to update stock and prices
            var product = await _db.Product.FirstOrDefaultAsync(p => p.ProductId == model.ProductId);
            if (product == null)
            {
                ModelState.AddModelError("", "Invalid product selected.");
                return View(model);
            }

            // Update the Sell model with product details
            model.ProductName = product.ProductName;
            model.BuyPrice = product.BuyPrice;
            model.Stock = product.Stock;
            model.TotalPrice = model.Quantity * model.SellingPrice;
            model.TotalTotalPrice += model.TotalPrice; // Increment total price for multiple items

            // Update the stock after selling
            product.Stock -= model.Quantity;
            if (product.Stock < 0)
            {
                ModelState.AddModelError("", "Insufficient stock for this product.");
                return View(model);
            }

            // Fetch customer information
            var customer = await _db.Customer.FirstOrDefaultAsync(c => c.Id == model.CustomerId);
            if (customer == null)
            {
                ModelState.AddModelError("", "Invalid customer selected.");
                return View(model);
            }

            // Update customer information in Sell model
            model.CustomerName = customer.Name;
            model.CustomerPhoneNo = customer.PhoneNo;
            model.CustomerAddress = customer.Address;
            model.DuePrice = customer.Due;  // Get customer's due amount

            // Calculate total due after this transaction
            model.TotalDuePrice = model.TotalTotalPrice - model.Deposit;

            // Save the Sell transaction to the database
            _db.Sell.Add(model);
            await _db.SaveChangesAsync();

            // Update the customer's due amount
            customer.Due += model.TotalDuePrice;
            await _db.SaveChangesAsync();
            //Toaster alert
            TempData["success"] = "Successfullly sold!.";
            // Redirect after successful transaction
            return RedirectToAction("ProductSell");
        }

        // Endpoint to fetch product details using ProductId (AJAX handler)
        [HttpGet]
        public async Task<JsonResult> GetProductDetails(int productId)
        {
            var product = await _db.Product
                .Where(p => p.ProductId == productId)
                .Select(p => new
                {
                    p.BuyPrice,
                    p.Stock
                })
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return Json(new { success = false, message = "Product not found" });
            }

            return Json(new { success = true, data = product });
        }

        // Endpoint to fetch customer details using CustomerId (AJAX handler)
        [HttpGet]
        public async Task<JsonResult> GetCustomerDetails(int customerId)
        {
            var customer = await _db.Customer
                .Where(c => c.Id == customerId)
                .Select(c => new
                {
                    c.PhoneNo,
                    c.Address,
                    c.Due
                })
                .FirstOrDefaultAsync();

            if (customer == null)
            {
                return Json(new { success = false, message = "Customer not found" });
            }

            return Json(new { success = true, data = customer });
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

                // Redirect to a confirmation page or customer list after successful addition
                return RedirectToAction("CustomerList");  // Assuming you have a CustomerList page
            }

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

        // GET: CustomerDue
        public IActionResult CustomerDue(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Fetch the customer details by ID
            var customer = _db.Customer.FirstOrDefault(c => c.Id == id);
            if (customer == null)
            {
                return NotFound();
            }

            // Pass the customer details to the view
            return View(customer);
        }

        // POST: CustomerDue
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CustomerDue(int id, decimal? dueClose)
        {
            var customer = _db.Customer.FirstOrDefault(c => c.Id == id);
            if (customer == null)
            {
                return NotFound();
            }

            if (dueClose == null || dueClose <= 0)
            {
                ModelState.AddModelError(nameof(dueClose), "Due Close amount must be greater than 0.");
            }

            if (ModelState.IsValid)
            {
                // Deduct the dueClose amount from the actual Due
                if (customer.Due.HasValue && dueClose.HasValue)
                {
                    customer.Due -= dueClose.Value;
                    customer.DueClose = dueClose;
                    customer.CreatedAt = DateTime.UtcNow;

                    // Update customer details in the database
                    _db.Customer.Update(customer);
                    _db.SaveChanges();

                    return RedirectToAction("CustomerDue", new { id = customer.Id });
                }
            }

            // If validation fails, redisplay the form
            return View("CustomerDue", customer);
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

                return View(); // This stays on the same page
            }
            // If model state is not valid, return the form with validation errors
            return View(product);

        }

        public ActionResult ProductBuyList()
        {
            var products = _db.Product.ToList();
            return View(products);
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
