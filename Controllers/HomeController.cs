using System.Diagnostics;
using System.Globalization;
using System.IO.Pipelines;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using POS.Migrations;
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

        // Method for fetching the logged-in user ID
        private int? GetLoggedInUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdString, out int userId))
            {
                return userId;
            }
            return null;
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

        // Private method to load products and customers
        private async Task<ProductCustomerViewModel> LoadProductCustomerViewModelAsync()
        {
            int? userId = GetLoggedInUserId();
            if (userId == null)
            {
                return null; // Return null if the user ID is not valid
            }

            // Fetch products and customers associated with the logged-in user
            var products = await _db.Product.Where(p => p.UserId == userId).ToListAsync();
            var customers = await _db.Customer.Where(c => c.UserId == userId).ToListAsync();

            // Initialize and return the view model
            return new ProductCustomerViewModel
            {
                Products = products,
                Customers = customers,
                Sells = new Sell
                {
                    ProductName = string.Empty,
                    Quantity = 0,
                    TotalPrice = 0m,
                    TotalTotalPrice = 0m,
                    ShabekDue = 0
                }
            };
        }
        // GET: ViewModelForProductCustomer
        public async Task<IActionResult> ViewModelForProductCustomer()
        {
            var viewModel = await LoadProductCustomerViewModelAsync();
            if (viewModel == null)
            {
                return BadRequest("Unable to load products and customers.");
            }
            return View(viewModel);
        }


        // GET: Fetches product details for a specific product ID (for AJAX)
        [HttpGet]
        public async Task<IActionResult> GetProductDetails(int productId)
        {
            int? userId = GetLoggedInUserId(); // Retrieve the logged-in user ID
            if (userId == null)
            {
                return Json(new { success = false, message = "User not authenticated." });
            }

            // Fetch the product associated with the logged-in user
            var product = await _db.Product
                .FirstOrDefaultAsync(p => p.ProductId == productId && p.UserId == userId);

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
                        Stock = product.Stock,
                        Unit = product.Unit
                    }
                });
            }

            return Json(new { success = false, message = "Product not found." });
        }


        [HttpGet]
        public async Task<IActionResult> SearchCustomers(string term)
        {
            var customers = await _db.Customer
                .Where(c => c.Name.Contains(term))
                .Select(c => new
                {
                    id = c.Id,
                    name = c.Name,
                    phoneNo = c.PhoneNo,
                    address = c.Address,
                    shabekDue = c.Due
                })
                .ToListAsync();

            return Json(new { success = true, customers });
        }
        // AJAX call to fetch customer details
        [HttpGet]
        public async Task<JsonResult> GetCustomerDetails(int customerId)
        {
            int? userId = GetLoggedInUserId(); // Retrieve the logged-in user ID
            if (userId == null)
            {
                return Json(new { success = false, message = "User not authenticated." });
            }

            // Fetch the customer associated with the logged-in user
            var customer = await _db.Customer
                .FirstOrDefaultAsync(c => c.Id == customerId && c.UserId == userId);

            if (customer != null)
            {
                return Json(new
                {
                    success = true,
                    data = new
                    {
                        CustomerId = customer.Id,
                        CustomerName = customer.Name,
                        Address = customer.Address,
                        PhoneNo = customer.PhoneNo,
                        ShabekDue = customer.Due
                    }
                });
            }

            return Json(new { success = false, message = "Customer not found!" });
        }


        // GET: ProductSell - Displays the sell page
        [Authorize(Roles = "Client")]
        [HttpGet]
        public async Task<IActionResult> ProductSell()
        {
            var viewModel = await LoadProductCustomerViewModelAsync();
            if (viewModel == null)
            {
                return BadRequest("Unable to load products and customers.");
            }
            return View(viewModel);
        }

        [IgnoreAntiforgeryToken]
        [HttpPost]
        public async Task<IActionResult> ProductSell([FromForm] ProductCustomerViewModel viewModel, [FromForm] int[] SelectedProductIds)
        {
            if (viewModel == null)
            {
                return BadRequest("ViewModel is null");
            }

            if (viewModel.Products == null || !viewModel.Products.Any())
            {
                return BadRequest("Products list is null or empty.");
            }

            if (SelectedProductIds == null || !SelectedProductIds.Any())
            {
                return BadRequest("No products were selected.");
            }

            using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    // Fetch the selected customer, if applicable
                    Customer customer = null;
                    if (viewModel.SelectedCustomerId > 0)
                    {
                        customer = await _db.Customer.FirstOrDefaultAsync(c => c.Id == viewModel.SelectedCustomerId);
                        if (customer == null)
                        {
                            ModelState.AddModelError("", "Invalid customer selected.");
                            return BadRequest(ModelState);
                        }
                    }
                    //creating a unique invoice number
                    string invoice = $"INV{DateTime.Now:yyyyMMddHHmmss}"; // Generate a unique invoice number
                    // Initialize variables to store combined product information
                    string totalSellingPrice = "";
                    decimal totalTotalPrice = 0;
                    string productNames = "";
                    string totalQuantity = "";
                    string productIds = "";

                    // Split the received comma-separated values into arrays
                    var quantities = viewModel.Quantities?.Split(',').Select(int.Parse).ToList();
                    var sellingPrices = viewModel.SellingPrices?.Split(',').Select(decimal.Parse).ToList();

                    if (quantities == null || sellingPrices == null || quantities.Count != sellingPrices.Count)
                    {
                        ModelState.AddModelError("", "Invalid data received for quantities or selling prices.");
                        return BadRequest(ModelState);
                    }
                    // Loop through all selected products and accumulate information
                    for (int i = 0; i < viewModel.Products.Count; i++)
                    {
                        var productSell = viewModel.Products[i]; // Now includes Quantity, TotalPrice, etc.
                        int selectedProductId = SelectedProductIds[i];

                        var product = await _db.Product.FirstOrDefaultAsync(p => p.ProductId == selectedProductId);
                        if (product == null)
                        {
                            ModelState.AddModelError("", $"Invalid product selected for Product ID: {selectedProductId}.");
                            return BadRequest(ModelState);
                        }
                        // Check stock availability                            
                        if (product.Stock < productSell.Stock)
                        {
                            ModelState.AddModelError("", $"Insufficient stock for product '{product.ProductName}'");
                            return BadRequest(ModelState);
                        }

                        // Reduce stock
                        product.Stock = productSell.Stock; 

                        //======== Update the product table in the database ========
                        _db.Product.Update(product);

                        // Combine product details for a single row
                        productIds += $"{product.ProductId}, "; // Concatenate product IDs
                        totalSellingPrice += $"{sellingPrices[i]}, ";
                        totalQuantity += $"{quantities[i]}, ";
                        totalTotalPrice += viewModel.TotalPrice;                      
                        productNames += $"{productSell.ProductName}, "; // Concatenate product names
                    }

                    // Trim the trailing comma and space from the product names
                    productNames = productNames.TrimEnd(',', ' ');
                    productIds = productIds.TrimEnd(',', ' ');
                    totalSellingPrice = totalSellingPrice.TrimEnd(',', ' ');
                    totalQuantity = totalQuantity.TrimEnd(',', ' ');


                    int? userId = GetLoggedInUserId();
                    if (userId == null)
                    {
                        return Unauthorized("User not authenticated."); // Return an unauthorized response if the user is not authenticated
                    }

                    // Create and populate the Sell entity with combined product data
                    var sell = new Sell
                    {
                        CustomerId = customer?.Id,
                        ProductName = productNames,
                        Quantity = 0,
                        SellingPrice = 0,
                        TotalPrice = totalTotalPrice,
                        TotalTotalPrice = viewModel.TotalTotalPrice, // This is the overall total price including any extra charges or adjustments
                        CustomerName = customer?.Name ?? "Walk-in Customer",
                        CustomerPhoneNo = customer?.PhoneNo ?? "N/A",
                        CustomerAddress = customer?.Address ?? "N/A",
                        DuePrice = viewModel.ShabekDue,
                        Deposit = viewModel.Deposit,
                        ShabekDue = viewModel.ShabekDue,
                        TotalDuePrice = viewModel.TotalTotalPrice + viewModel.ShabekDue - viewModel.Deposit,
                        UserId = userId,
                        ProductIds = productIds,
                        ProductNames = productNames,
                        Quantities = totalQuantity,
                        TotalPricePerProduct = totalSellingPrice,
                        Invoice = invoice
                    };
                    //======== Add data to Sell table ========
                    _db.Sell.Add(sell);

                    // Update customer due if customer is selected
                    if (customer != null)
                    {
                        customer.Due = sell.TotalDuePrice;

                        // Update payment information for the customer
                        customer.PaymentDates = string.IsNullOrEmpty(customer.PaymentDates)
                            ? DateTime.Now.ToString("yyyy-MM-dd")
                            : $"{customer.PaymentDates},{DateTime.Now:yyyy-MM-dd}";

                        customer.PaymentAmounts = string.IsNullOrEmpty(customer.PaymentAmounts)
                            ? viewModel.Deposit.ToString()
                            : $"{customer.PaymentAmounts},{viewModel.Deposit}";
                        
                        
                        //======== Update data to Customer table ========
                        _db.Customer.Update(customer);
                    }
                    else if (customer == null && sell.TotalDuePrice > 0)
                    {
                        customer = new Customer
                        {
                            Name = "Walk-in Customer",
                            PhoneNo = "N/A",
                            Address = "N/A",
                            Due = sell.TotalDuePrice,
                            UserId = userId,
                            FirstDue = sell.TotalDuePrice,
                            Invoice = invoice
                        };
                        //======== Add data to Customer table ========
                        _db.Customer.Add(customer);
                    }
                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["success"] = "Successfully sold!";
                    //return RedirectToAction("ProductSell");
                    // If successful
                    return Json(new { success = true, message = "Successfully sold!",
                        invoiceDetails = new
                        {
                            invoiceNumber = invoice,
                            customerName = customer?.Name ?? "Walk-in Customer",
                            totalAmount = sell.TotalTotalPrice,
                            totalDue = sell.TotalDuePrice
                        }
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "An error occurred while saving data: " + ex.Message);
                    //return BadRequest(ModelState);
                    // In case of failure
                    return Json(new { success = false, message = "An error occurred." });
                }
            }
        }

        public async Task<IActionResult> ProductSellReport()
        {
            // Retrieve the logged-in user ID
            int? userId = GetLoggedInUserId();
            if (userId == null)
            {
                return Unauthorized("User not authenticated."); // Return an unauthorized response if the user is not authenticated
            }

            // Fetch all sell records with related product and customer details, filtered by user ID
            var sellReport = await _db.Sell
                .Where(s => s.UserId == userId)
                .Include(s => s.Product)
                .Include(s => s.Customer)
                .ToListAsync();

            // Pass the data directly to the view
            return View(sellReport);
        }

        // GET: Sell/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> EditProductSell(int id)
        {
            var sell = await _db.Sell
                       .Include(s => s.Product)
                       .Include(s => s.Customer)
                       .FirstOrDefaultAsync(s => s.SellId == id);

            if(sell == null)
            {
                return NotFound();
            }
            var products = await _db.Product.ToListAsync();
            var customers = await _db.Customer.ToListAsync();

            if(products == null || customers == null)
            {
                return BadRequest("No products or customers found.");
            }
            var productIds = sell.ProductIds?.Split(',').Select(int.Parse).ToList() ?? new List<int>();
            var productNames = sell.ProductNames?.Split(',').ToList() ?? new List<string>();
            var quantities = sell.Quantities?.Split(',').Select(int.Parse).ToList() ?? new List<int>();
            var pricePerProduct = sell.TotalPricePerProduct?.Split(',').Select(decimal.Parse).ToList() ?? new List<decimal>();
            
            var productDetailsList = new List<ProductDetailsViewModel>();

            for (int i = 0; i < productIds.Count; i++)
            {
                var productDetail = new ProductDetailsViewModel
                {
                    ProductId = productIds[i],
                    ProductName = productNames.Count > i ? productNames[i] : string.Empty,
                    Quantity = quantities.Count > i ? quantities[i] : 0,
                    TotalPrice = pricePerProduct.Count > i ? pricePerProduct[i] : 0.0m
                };
                productDetailsList.Add(productDetail);
            }

            // Prepare the ViewModel with the parsed data and other existing data
            var viewModel = new ProductCustomerViewModel
            {
                Sells = sell,
                Products = products,
                Customers = customers,
                ProductDetails = productDetailsList,
                SelectedCustomerId = sell.CustomerId ?? 0,
                SelectedProductId = sell.ProductId ?? 0,
                Invoice = sell.SellId,
                CustomerPhoneNo = sell.CustomerPhoneNo,
                CustomerAddress = sell.CustomerAddress,
                Deposit = sell.Deposit,
                ShabekDue = sell.ShabekDue ?? 0
            };

            return View(viewModel);

        }

        // POST: Sell/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProductSell(int id, ProductCustomerViewModel model)
        {
            if (id != model.Sells?.SellId)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                // Retrieve the existing sell entry
                var sellToUpdate = await _db.Sell.FindAsync(id);
                if (sellToUpdate == null)
                {
                    return NotFound();
                }

                // Retrieve the associated customer and product
                var customerToUpdate = await _db.Customer.FindAsync(sellToUpdate.CustomerId);
                var productToUpdate = await _db.Product.FindAsync(sellToUpdate.ProductId);

                if (customerToUpdate == null || productToUpdate == null)
                {
                    return NotFound();
                }

                // Update the Sell details
                sellToUpdate.ProductName = model.ProductName;
                sellToUpdate.Quantity = model.Quantity;
                sellToUpdate.BuyPrice = model.BuyPrice;
                sellToUpdate.SellingPrice = model.SellingPrice;
                sellToUpdate.TotalPrice = model.TotalPrice;
                sellToUpdate.TotalTotalPrice = model.TotalTotalPrice;
                sellToUpdate.Deposit = model.Deposit;
                sellToUpdate.ShabekDue = model.ShabekDue;
                sellToUpdate.CustomerPhoneNo = model.CustomerPhoneNo;
                sellToUpdate.CustomerAddress = model.CustomerAddress;

                // Update the Customer details if necessary
                customerToUpdate.Name = model.Name;
                customerToUpdate.PhoneNo = model.CustomerPhoneNo;
                customerToUpdate.Address = model.CustomerAddress;

                // Save the changes to the database
                try
                {
                    _db.Update(sellToUpdate);
                    _db.Update(customerToUpdate);
                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SellExists(sellToUpdate.SellId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index)); // Redirect to the index or another view after editing
            }

            // Re-populate the ViewModel if the model state is invalid
            model.Products = await _db.Product.ToListAsync();
            model.Customers = await _db.Customer.ToListAsync();

            return View(model);
        }

        [HttpGet]
        public IActionResult DetailsProductSell(int id)
        {
            var sellDetails = _db.Sell
                .Include(s => s.Product)
                .Include(s => s.Customer)
                .Include(s => s.User)
                .FirstOrDefault(s => s.SellId == id);

            if (sellDetails == null)
            {
                return NotFound();
            }

            return View(sellDetails);
        }


        private bool SellExists(int id)
        {
            return _db.Sell.Any(e => e.SellId == id);
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
            // Retrieve the logged-in user ID
            int? userId = GetLoggedInUserId();
            if (userId == null)
            {
                return Unauthorized("User not authenticated."); // Return an unauthorized response if the user is not authenticated
            }

            // Fetch all customers associated with the logged-in user
            var customers = await _db.Customer
                .Where(c => c.UserId == userId) // Filter customers by user ID
                .ToListAsync();

            // Pass the customers to the view
            return View(customers);
        }

        // GET: CustomerDue (Displays the dropdown with all customers)
        public IActionResult CustomerDue()
        {
            // Retrieve the logged-in user ID
            int? userId = GetLoggedInUserId();
            if (userId == null)
            {
                return Unauthorized("User not authenticated."); // Return an unauthorized response if the user is not authenticated
            }

            // Fetch customers associated with the logged-in user
            var customers = _db.Customer
                .Where(c => c.UserId == userId) // Filter customers by user ID
                .ToList();

            // Pass the filtered customers to the dropdown
            ViewBag.CustomerList = new SelectList(customers, "Id", "Name");
            return View();
        }


        // GET: Get customer details by ID using AJAX
        [HttpGet]
        public async Task<JsonResult> GetCustomerDetailsForDueCustomer(int id)
        {
            // Retrieve the logged-in user ID
            int? userId = GetLoggedInUserId();
            if (userId == null)
            {
                return Json(new { success = false, message = "User not authenticated." });
            }

            // Fetch the customer based on the ID and user ID
            var customer = await _db.Customer
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId); // Ensure the customer belongs to the logged-in user

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
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int? userId = GetLoggedInUserId();

            // Now assign it to customer.UserId
            customer.UserId = userId;

            // Proceed with updating the customer details if ModelState is valid
            customer.Due -= model.DueClose.Value; // Ensure DueClose is not null with .Value
            customer.DueClose = model.DueClose;

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

            // Fetch the list of clients from the database
            var clientList = _db.Client.ToList();

            // Check if the client list is not empty
            if (clientList == null || !clientList.Any())
            {
                // Return an empty list or handle the case where no clients are found
                clientList = new List<Client>();
            }

            // Pass the client list to the view using ViewBag
            ViewBag.ClientList = clientList;

            return View(new ProductClientViewModel());
        }
        [HttpGet]

        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            var userId = GetLoggedInUserId();
            if (userId == null)
            {
                return Json(new { success = false, message = "User ID not found" });
            }

            var products = await _db.Product
                .Where(p => p.UserId == userId) // Assuming a UserId column in Product table
                .Select(p => new
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    BuyPrice = p.BuyPrice,
                    Stock = p.Stock,
                    Unit = p.Unit
                })
                .ToListAsync();


            return Json(new { success = true, products });
        }
        public async Task<IActionResult> GetClientDetails(int id)
        {
            var userId = GetLoggedInUserId();
            if (userId == null)
            {
                return Json(new { success = false, message = "User ID not found" });
            }

            var clients = await _db.Client
                .Where(c => c.UserId == userId) // Assuming a UserId column in Client table
                .Select(c => new
                {
                    clientId = c.Id,
                    name = c.Name,
                    address = c.Address,
                    phoneNo = c.PhoneNo,
                    debt = c.Debt
                })
                .ToListAsync();

            return Json(new { success = true, clients });
        }

        [IgnoreAntiforgeryToken]
        [HttpPost]
        public async Task<IActionResult> ProductBuy([FromForm] ProductClientViewModel viewModel)
        {
            if (viewModel == null)
            {
                return BadRequest("ViewModel is null");
            }

            var productIds = viewModel.ProductIds?.Split(',').Select(id => int.TryParse(id, out var parsedId) ? parsedId : (int?)null).ToList();
            var quantities = viewModel.Quantities?.Split(',').Select(int.Parse).ToList();
            var productNames = viewModel.ProductNames?.Split(',');
            var previousBuyPrices = viewModel.PreviousBuyPrices?.Split(',').Select(int.Parse).ToList();
            var buyPricePerProduct = viewModel.BuyPricePerProduct?.Split(',').Select(int.Parse).ToList();
            if (quantities == null || productIds == null || productNames == null || quantities.Count != productIds.Count || productIds.Count != productNames.Length)
            {
                return BadRequest("Invalid product data.");
            }

            int? userId = GetLoggedInUserId();
            if (userId == null)
            {
                return Unauthorized("User not authenticated.");
            }

            using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    string invoice = $"INV{DateTime.Now:yyyyMMddHHmmss}";
                    decimal totalBuyPrice = 0;
                    string productIdsString = "";
                    string productNamesString = "";
                    string quantitiesString = "";
                    string previouBPString = "";
                    string buyPPPString = "";

                    for (int i = 0; i < productIds.Count; i++)
                    {
                        var productId = productIds[i];
                        var productName = productNames[i];
                        var quantity = quantities[i];
                        var prevBuyP = previousBuyPrices[i];
                        var buyPrice = buyPricePerProduct[i];
                        Product product;

                        // Check if product exists or create a new product entry
                        if (productId.HasValue)
                        {
                            product = await _db.Product.FirstOrDefaultAsync(p => p.ProductId == productId.Value);
                            if (product == null)
                            {
                                return BadRequest($"Invalid product ID: {productId.Value}");
                            }
                        }
                        else
                        {
                            // Add new product if ProductId is null and ProductName is available
                            product = new Product { ProductName = productName, Stock = quantity, CreatedAt = DateTime.Now, BuyPrice= buyPrice };
                            await _db.Product.AddAsync(product);
                            await _db.SaveChangesAsync();
                        }

                        // Update product stock and total buy price
                        product.Stock += quantity;
                        //decimal buyPrice = product.BuyPrice;
                        //totalBuyPrice += buyPrice * quantity;
                        product.BuyPrice = buyPrice;
                        product.PreviousBuyPrice = buyPrice;

                        // Concatenate product information
                        productIdsString += $"{product.ProductId}, ";
                        productNamesString += $"{product.ProductName}, ";
                        quantitiesString += $"{quantity}, ";
                        previouBPString += $"{prevBuyP}, ";
                        buyPPPString += $"{buyPrice}, ";

                        product.EmployeeId = userId;
                        product.UserId = userId;
                        // Append new stock date and amount to the product fields
                        // Update the AddStockDates and AddStockAmounts by appending the new data
                        var currentDate = DateTime.Now.ToString("yyyy-MM-dd");
                        product.AddStockDates = string.IsNullOrEmpty(product.AddStockDates)
                            ? currentDate
                            : $"{product.AddStockDates},{currentDate}";

                        product.AddStockAmounts = string.IsNullOrEmpty(product.AddStockAmounts)
                            ? quantity.ToString()
                            : $"{product.AddStockAmounts},{quantity}";

                        // Update the product
                        _db.Product.Update(product);
                    }

                    // Remove the trailing comma and space
                    productNamesString = productNamesString.TrimEnd(',', ' ');
                    productIdsString = productIdsString.TrimEnd(',', ' ');
                    quantitiesString = quantitiesString.TrimEnd(',', ' ');
                    previouBPString = previouBPString.TrimEnd(',', ' ');
                    buyPPPString = buyPPPString.TrimEnd(',', ' ');

                    // Handle client information
                    Client client = null;
                    if (viewModel.ClientId.HasValue)
                    {
                        client = await _db.Client.FirstOrDefaultAsync(c => c.Id == viewModel.ClientId.Value);
                        if (client == null)
                        {
                            return BadRequest($"Invalid client ID: {viewModel.ClientId.Value}");
                        }
                        
                    }
                    else if (!string.IsNullOrWhiteSpace(viewModel.ClientName))
                    {
                        // Create new client if ClientId is null and ClientName is provided
                        client = new Client
                        {
                            Name = viewModel.ClientName,
                            Address = viewModel.ClientAddress,
                            PhoneNo = viewModel.ClientPhoneNo,
                            Debt = viewModel.ShabekDue,
                            UserId = userId// Initial debt can be set here if necessary
                        };
                        await _db.Client.AddAsync(client);
                        await _db.SaveChangesAsync();
                    }

                    // Update client debt
                    if (client != null)
                    {
                        client.Debt = viewModel.ShabekDue;
                        client.AddDebtDates = string.IsNullOrEmpty(client.AddDebtDates)
                            ? DateTime.Now.ToString("yyyy-MM-dd")
                            : $"{client.AddDebtDates},{DateTime.Now:yyyy-MM-dd}";

                        //==========Jhamela ase ekhane ==========
                        client.AddDebtAmounts = string.IsNullOrEmpty(client.AddDebtAmounts)
                            ? viewModel.ShabekDue.ToString()
                            : $"{client.AddDebtAmounts},{viewModel.ShabekDue}";
                        
                        _db.Client.Update(client);
                    }

                    // Create the Buy entity
                    var buy = new Buy
                    {
                        Invoice = invoice,
                        ProductIds = productIdsString,
                        ProductNames = productNamesString,
                        PreviousBuyPrices = previouBPString,
                        BuyPricePerProduct = buyPPPString,
                        Quantities = quantitiesString,
                        ClientId = client?.Id,
                        UserId = userId,
                        TotalSum = totalBuyPrice,
                        ShabekDue = viewModel.ShabekDue,
                        Deposit = viewModel.Deposit,
                        ClientName = viewModel.ClientName ?? "Walk-in Client",
                        ClientAddress = viewModel.ClientAddress ?? "N/A",
                        ClientPhoneNo = viewModel.ClientPhoneNo ?? "N/A"
                    };

                    // Add the new buy record to the database
                    _db.Buy.Add(buy);

                    // Save all changes
                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Json(new { success = true, message = "Purchase recorded successfully!", invoice });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = "An error occurred while saving data: " + ex.Message });
                }
            }
        }



        public async Task<IActionResult> ProductBuyList()
        {
            // Retrieve the logged-in user ID
            int? userId = GetLoggedInUserId();
            if (userId == null)
            {
                // Optionally, you could return an error view or redirect to a login page
                return RedirectToAction("Login", "RegLog"); // Redirect to login if user is not authenticated
            }

            // Fetch products associated with the logged-in user
            var products = await _db.Product
                .Where(p => p.UserId == userId) // Ensure the product belongs to the logged-in user
                .ToListAsync();

            return View(products);
        }


        [HttpGet]
        public async Task<IActionResult> UpdateProductStock(int id)
        {
            // Retrieve the logged-in user ID
            int? userId = GetLoggedInUserId();
            if (userId == null)
            {
                // Optionally, redirect to the login page if user is not authenticated
                return RedirectToAction("Login", "RegLog");
            }

            // Find the product by ID and check if it belongs to the logged-in user
            var product = await _db.Product.FirstOrDefaultAsync(p => p.ProductId == id && p.UserId == userId);

            if (product == null)
            {
                return NotFound(); // Product not found or does not belong to the user
            }

            // Pass the product details to the view
            return View(product);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProductStock(Product model)
        {
            // Find the product by ID
            var product = _db.Product.FirstOrDefault(p => p.ProductId == model.ProductId);
            if (product == null)
            {
                return NotFound();
            }

            // Validate the AddStock field
            if (model.AddStock == null || model.AddStock <= 0)
            {
                ModelState.AddModelError(nameof(model.AddStock), "Stock amount must be greater than 0.");
            }

            // Validate BuyPrice
            if (model.BuyPrice <= 0)
            {
                ModelState.AddModelError(nameof(model.BuyPrice), "Buy price must be greater than 0.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Update stock
            product.Stock += model.AddStock.Value;

            // Get the current stock date and amount
            string currentDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"); // Include time for more precision
            string currentAmount = model.AddStock.Value.ToString(); // Convert the added stock to string

            // Append the current date to the AddStockDates column
            product.AddStockDates = string.IsNullOrWhiteSpace(product.AddStockDates)
                ? currentDate
                : product.AddStockDates + "," + currentDate;

            // Append the added stock amount to the AddStockAmounts column
            product.AddStockAmounts = string.IsNullOrWhiteSpace(product.AddStockAmounts)
                ? currentAmount
                : product.AddStockAmounts + "," + currentAmount;

            // Optionally update the BuyPrice if needed
            product.BuyPrice = model.BuyPrice;

            // Save changes to the database with transaction
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    _db.Product.Update(product);
                    _db.SaveChanges();
                    transaction.Commit(); // Commit transaction if everything is successful
                    TempData["success"] = "Successfully updated product stock details!";
                    return RedirectToAction("ProductBuyList"); // Or another page where you want to redirect
                }
                catch (Exception ex)
                {
                    transaction.Rollback(); // Rollback in case of any failure
                    TempData["error"] = "An error occurred while updating the database!";
                    ModelState.AddModelError("", "An error occurred while updating the database");
                    return View(model); // Return the view with errors
                }
            }
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