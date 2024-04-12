using IntexII_Project_4_2.Data;
using IntexII_Project_4_2.Models;
using IntexII_Project_4_2.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.OnnxRuntime;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntexII_Project_4_2.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly InferenceSession _session;
        public readonly string _onnxModelPath;

        public AdminController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _onnxModelPath = System.IO.Path.Combine(hostEnvironment.WebRootPath, "Final_Model.onnx");
            _session = new InferenceSession(_onnxModelPath);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult AddProduct()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult AddProduct(Product product)
        {
            if (ModelState.IsValid)
            {
                if (product.ProductId == 0)
                {
                    _context.Products.Add(product);
                    _context.SaveChanges();
                    return RedirectToAction("AllProducts");
                }
                else
                {
                    // Handle update logic or error as necessary
                }
            }
            return View(product);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult AddUser()
        {
            return View(new ApplicationUser());
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult CreateUser(ApplicationUser newUser)
        {
            if (ModelState.IsValid)
            {
                _context.Users.Add(newUser);
                _context.SaveChanges();
                return RedirectToAction("AllCustomerInfo");
            }
            return View("AddUser", newUser);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult AllProducts()
        {
            var products = _context.Products.ToList();
            return View(products);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult AllOrders(string filter = "all", int page = 1)
        {
            int pageSize = 50;
            IQueryable<Order> query = _context.Orders;

            switch (filter.ToLower())
            {
                case "unfulfilled":
                    query = query.Where(order => !order.Fullfilled);
                    break;
                case "fraud":
                    query = query.Where(order => order.Fraud > 0);
                    break;
                default:
                    break;
            }

            var filteredOrders = query.OrderBy(order => order.Date).Skip((page - 1) * pageSize).Take(pageSize).ToList();
            var totalOrders = query.Count();

            var pageInfo = new PaginationInfo
            {
                CurrentPage = page,
                ItemsPerPage = pageSize,
                TotalItems = totalOrders
            };

            var viewModel = new OrderListViewModel
            {
                Orders = filteredOrders,
                PaginationInfo = pageInfo,
                CurrentFilter = filter
            };

            return View(viewModel);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult DeleteProduct(int productId)
        {
            var product = _context.Products.Find(productId);
            if (product != null)
            {
                _context.Products.Remove(product);
                _context.SaveChanges();
                return RedirectToAction("AllProducts");
            }
            return NotFound();
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult ConfirmDeleteUser(string id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }

            return View("DeleteUserConfirmation", user); // Assuming the view name is ConfirmDeleteUser
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult DeleteUser(string id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            _context.SaveChanges();
            return RedirectToAction("AllCustomerInfo");
        }
        [Authorize(Roles = "Admin")]
        public IActionResult EditConfirmation(Product product)
        {
            return View(product);  // Display the confirmation view
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult EditCustomerInfo(string id)
        {
            var customer = _context.Users.FirstOrDefault(u => u.Id == id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult EditOrder(int id)
        {
            var order = _context.Orders.FirstOrDefault(o => o.TransactionId == id);
            if (order == null)
            {
                return NotFound();
            }

            // Assuming CustomerId is not nullable, adjust if it's nullable
            var customer = order.CustomerId != null ? _context.Customers.FirstOrDefault(c => c.CustomerId == order.CustomerId) : null;

            var viewModel = new EditOrderViewModel
            {
                TransactionId = order.TransactionId,
                Date = order.Date,
                Time = order.Time,
                // Amount = order?.Amount,
                CountryOfTransaction = order.CountryOfTransaction,
                ShippingAddress = order.ShippingAddress,
                Bank = order.Bank,
                TypeOfCard = order.TypeOfCard,
                CustomerId = order.CustomerId ?? 0,
                FirstName = customer?.FirstName,
                LastName = customer?.LastName,
                CountryOfResidence = customer?.CountryOfResidence,
                IsFraudulent = order.Fraud > 0,
                IsFullfilled = order.Fullfilled
            };

            return View(viewModel);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult EditOrder(EditOrderViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var order = _context.Orders.Find(viewModel.TransactionId);
                if (order != null)
                {
                    // Update the order properties
                    order.Fullfilled = viewModel.IsFullfilled;
                    order.Fraud = viewModel.IsFraudulent ? 1 : 0; // Assuming the Fraud is stored as an integer

                    _context.SaveChanges();

                    // Redirect to the AllOrders view after the update
                    return RedirectToAction("AllOrders");
                }
                else
                {
                    return NotFound();
                }
            }

            // If validation fails, redisplay the form with the current view model
            return View(viewModel);
        }
        [Authorize(Roles = "Admin")]
        public IActionResult EditProduct(int id)
        {
            Product product;
            if (id == 0)  // Assuming 0 or a negative number indicates a new product
            {
                product = new Product();
            }
            else
            {
                product = _context.Products.FirstOrDefault(p => p.ProductId == id);
                if (product == null)
                {
                    return NotFound();
                }
            }
            return View(product);
        }

        // POST: Update the product in the database
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult EditProduct(Product product)
        {
            if (ModelState.IsValid)
            {
                // Update logic here
                _context.Update(product);
                _context.SaveChanges();

                return RedirectToAction("AllProducts"); // Redirect to the AllProducts view
            }

            // Return back to the edit form if there are any validation errors
            return View(product);
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            var endDate = DateTime.Today;
            var startDate = endDate.AddDays(-7);
            string[] dateFormats = { "M/d/yyyy", "MM/dd/yyyy", "M/dd/yyyy", "MM/d/yyyy" };

            var orders = _context.Orders.ToList(); // Assuming _context is your database context

            var totalSales2023 = orders
                .Where(o => DateTime.TryParseExact(o.Date, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate) && parsedDate.Year == 2023)
                .Sum(o => o.Amount);

            var totalSalesPast7Days = orders
                .Where(o => DateTime.TryParseExact(o.Date, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate) && parsedDate >= startDate && parsedDate <= endDate)
                .Sum(o => o.Amount);

            var unfulfilledOrders = orders
                .Count(o => !o.Fullfilled && DateTime.TryParseExact(o.Date, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out _));

            var ordersFulfilledPast7Days = orders
                .Count(o => o.Fullfilled && DateTime.TryParseExact(o.Date, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate) && parsedDate >= startDate && parsedDate <= endDate);

            var viewModel = new AdminKPIViewModel
            {
                TotalSales2023 = (int)totalSales2023,
                TotalSalesPast7Days = (int)totalSalesPast7Days,
                UnfulfilledOrders = unfulfilledOrders,
                OrdersFulfilledPast7Days = ordersFulfilledPast7Days
            };

            return View(viewModel);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> UpdateCustomerInfo(ApplicationUser model)
        {
            var user = await _context.Users.FindAsync(model.Id);
            if (user == null)
            {
                return NotFound();
            }

            // Update the properties
            user.Email = model.Email;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Country = model.Country;
            user.Gender = model.Gender;
            user.DateOfBirth = model.DateOfBirth;
            user.TwoFactorEnabled = model.TwoFactorEnabled;

            // Save the changes
            _context.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index"); // Redirect to a suitable page
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult UpdateProduct(Product product)
        {
            if (ModelState.IsValid)
            {
                if (product.ProductId > 0)
                {
                    var existingProduct = _context.Products.Find(product.ProductId);
                    if (existingProduct != null)
                    {
                        _context.Entry(existingProduct).CurrentValues.SetValues(product);
                    }
                }
                else
                {
                    _context.Products.Add(product);
                }

                _context.SaveChanges();
                return RedirectToAction("AllProducts");
            }

            return View("EditProduct", product);  // Only if something goes wrong
        }
        [Authorize(Roles = "Admin")]
        public IActionResult AllCustomerInfo()
        {
            var customers = _context.Users.ToList(); // Retrieve all users from the database
            return View(customers);
        }
    }
}


