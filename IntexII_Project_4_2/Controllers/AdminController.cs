using IntexII_Project_4_2.Data;
using IntexII_Project_4_2.Models;
using IntexII_Project_4_2.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Globalization;

namespace IntexII_Project_4_2.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult AddProduct()
        {
            return View(new Product()); // Initialize a new product to be filled out
        }

        // POST: Process the AddProduct form submission
        [HttpPost]
        public IActionResult AddProduct(Product product)
        {
            if (ModelState.IsValid)
            {
                // Assuming ProductId is 0 or not set for new entries
                if (product.ProductId == 0) // This check might be redundant if ProductId is auto-incremented
                {
                    _context.Products.Add(product);
                    _context.SaveChanges(); // This should automatically generate ProductId for new entries
                    return RedirectToAction("AllProducts");
                }
                else
                {
                    // Handle update logic or error as necessary
                }
            }

            // If model state is invalid, render the form again
            return View(product);
        }

        public IActionResult AllProducts()
        {
            var products = _context.Products.ToList();
            return View(products);
        }

        public IActionResult AllOrders(string filter = "all", int page = 1)
        {
            int pageSize = 50; // Set the number of items per page
            IQueryable<Order> query = _context.Orders;

            // Apply filters based on the 'filter' parameter
            switch (filter.ToLower())
            {
                case "unfulfilled":
                    query = query.Where(order => !order.Fullfilled);
                    break;
                case "fraud":
                    query = query.Where(order => order.Fraud > 0);
                    break;
                case "all":
                default:
                    // No additional filter for 'all'
                    break;
            }

            var filteredOrders = query.ToList();

            // Sort by date and process pagination in memory
            var sortedOrders = filteredOrders
                .Select(order =>
                {
                    DateTime.TryParseExact(order.Date, new[] { "M/d/yyyy", "MM/dd/yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate);
                    return new { Order = order, ParsedDate = parsedDate };
                })
                .OrderByDescending(temp => temp.ParsedDate)
                .Select(temp => temp.Order)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var totalOrders = query.Count();

            var pageInfo = new PaginationInfo
            {
                CurrentPage = page,
                ItemsPerPage = pageSize,
                TotalItems = totalOrders
            };

            var viewModel = new OrderListViewModel
            {
                Orders = sortedOrders,
                PaginationInfo = pageInfo,
                CurrentFilter = filter
            };

            return View(viewModel);
        }
        public IActionResult Delete()
        {
            return View();
        }

        public IActionResult DeleteConfirmation(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost]
        public IActionResult DeleteProduct(int productId)
        {
            var product = _context.Products.Find(productId);
            if (product != null)
            {
                _context.Products.Remove(product);
                _context.SaveChanges();
            }

            return RedirectToAction("AllProducts");
        }

        public IActionResult EditConfirmation(Product product)
        {
            return View(product);  // Display the confirmation view
        }
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
                Amount = order.Amount,
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
                TotalSales2023 = totalSales2023,
                TotalSalesPast7Days = totalSalesPast7Days,
                UnfulfilledOrders = unfulfilledOrders,
                OrdersFulfilledPast7Days = ordersFulfilledPast7Days
            };

            return View(viewModel);
        }
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
    }
}
