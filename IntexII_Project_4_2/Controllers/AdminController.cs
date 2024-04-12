using IntexII_Project_4_2.Data;
using IntexII_Project_4_2.Models;
using IntexII_Project_4_2.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.OnnxRuntime;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace IntexII_Project_4_2.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly InferenceSession _session;
        public readonly string _onnxModelPath;

        public AdminController(ApplicationDbContext context, IHostEnvironment hostEnvironment)
        {
            _context = context;
            _onnxModelPath = System.IO.Path.Combine(hostEnvironment.ContentRootPath, "Final_Model.onnx");
            _session = new InferenceSession(_onnxModelPath);

        }


        public IActionResult AddProduct()
        {
            return View();
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

        public IActionResult AllOrdersCopy()
        {

            var records = (from order in _context.Orders
                          join customer in _context.Customers
                          on order.CustomerId equals customer.CustomerId
                          select new
                          {
                              Order = order,
                              Customer = customer
                          }).Take(5000);
            var predictions = new List<OrderPrediction>();  // Your ViewModel for the view

            // Dictionary mapping the numeric prediction to an animal type
            var class_type_dict = new Dictionary<int, string>
    {
        { 0, "not fraud" },
        { 1, "fraud" }
    };

            foreach (var record in records)
            {
                DateTime orderDate;
                bool isDateValid = DateTime.TryParseExact(record.Order.Date, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out orderDate);

                if (!isDateValid)
                {
                    continue; // Skip this record or handle it as needed
                }

                int dayOfMonth = orderDate.Day;
                int monthOfYear = orderDate.Month;

                // Prepare input values according to the expected model features
                var input = new List<float>
                {
                    (float)record.Customer.Age,
                    (float)record.Customer.CustomerId,
                    (float)record.Order.TransactionId,
                    (float)record.Order.Time, // Assuming Time is a single float value representing the transaction time
                    (float)(record.Order.Amount ?? 0), // Assuming Amount can be nullable and setting default to 0 if null
                    (float)dayOfMonth,
                    (float)monthOfYear,
                    record.Order.CountryOfTransaction == "United Kingdom" ? 1f : 0f, // Assuming binary encoding for country
                    (record.Order.ShippingAddress ?? record.Order.CountryOfTransaction) == "United Kingdom" ? 1f : 0f // Similarly for shipping address
                };

                var inputTensor = new DenseTensor<float>(input.ToArray(), new[] { 1, 9 }); // Adjust dimensions to match model expectation

                // Log the input tensor shape and data for verification
                Console.WriteLine($"Input Tensor Shape: [{inputTensor.Dimensions[0]}, {inputTensor.Dimensions[1]}]");
                Console.WriteLine($"Input Data: {string.Join(", ", input)}");


                var inputs = new List<NamedOnnxValue>
                {
                    NamedOnnxValue.CreateFromTensor("float_type", inputTensor) // Correcting the input name to match your model
                };


                string predictionResult;


                using (var results = _session.Run(inputs))
                {
                    var prediction = results.FirstOrDefault(item => item.Name == "output_label")?.AsTensor<long>().ToArray();
                    predictionResult = prediction != null && prediction.Length > 0 ? class_type_dict.GetValueOrDefault((int)prediction[0], "Unknown") : "Error in prediction";
                }

                predictions.Add(new OrderPrediction
                {
                    Order = record.Order,
                    Customer = record.Customer,
                    Prediction = predictionResult
                });
            }

            return View(predictions);

        }


        public IActionResult AllProducts()
        {
            return View();
        }
        public IActionResult Delete()
        {
            return View();
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
                // TotalSales2023 = totalSales2023,
                // TotalSalesPast7Days = totalSalesPast7Days,
                UnfulfilledOrders = unfulfilledOrders,
                OrdersFulfilledPast7Days = ordersFulfilledPast7Days
            };

            return View(viewModel);
        }
        public IActionResult UpdateProduct()
        {
            return View();
        }
    }
}
