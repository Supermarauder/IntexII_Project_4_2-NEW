using IntexII_Project_4_2.Data;
using IntexII_Project_4_2.Models;
using IntexII_Project_4_2.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Client;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace IntexII_Project_4_2.Controllers
{
    public class AuthUserController : Controller

    {
        private IIntexProjectRepository _repo;

        private readonly ApplicationDbContext _context;

        private readonly InferenceSession _session;
        public readonly string _onnxModelPath;

        public AuthUserController(IIntexProjectRepository repo, IWebHostEnvironment hostEnvironment, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _onnxModelPath = System.IO.Path.Combine(hostEnvironment.ContentRootPath, "model_final.onnx");
            _session = new InferenceSession(_onnxModelPath);
            _userManager = userManager;
        }

        //public IActionResult Checkout()
        //{
        //    return View();
        //}
        [Authorize(Roles = "Member")]

        public IActionResult Checkout()
        {

            var customerId = HttpContext.Session.GetInt32("CustomerId");

            var viewModel = new OrderPrediction
            {

                Cart = HttpContext.Session.GetJson<Cart>("cart"),
                CustomerId = customerId ?? default
            };

            return View(viewModel);

        }

        [Authorize(Roles = "Member")]
        // public IActionResult Confirmation1();

        [HttpPost]
        public async Task<IActionResult> Checkout(OrderPrediction viewModel)
        {
            ApplicationUser currentUser = await _userManager.GetUserAsync(User);
            
            var cart = HttpContext.Session.GetJson<Cart>("cart");
            var total = cart.CalculateTotal();

            // Set Order.Amount to the cart's total
            viewModel.Order.Amount = (int?)total;

            // Set additional Order properties
            var currentDateTime = DateTime.Now;
            viewModel.Order.Date = currentDateTime.ToString("MM/dd/yyyy");
            viewModel.Order.DayOfWeek = currentDateTime.ToString("ddd");
            viewModel.Order.Time = currentDateTime.Hour;

            // Add the order using the repository
            _repo.AddOrder(viewModel.Order);

            // Dictionary mapping the numeric prediction to a fraud status
            var class_type_dict = new Dictionary<int, string>
    {
        { 0, "not fraud" },
        { 1, "fraud" }
    };

            // Prepare variables for model pipeline inputs
            float customerId = (float)currentUser.CustomerId;
            float age = (float)viewModel.Customer.Age;
            //float customerId = (float)viewModel.Order.CustomerId;
            float transactionId = (float)viewModel.Order.TransactionId;
            float time = (float)viewModel.Order.Time;
            float amount = (float)viewModel.Order.Amount;
            float dayOfMonth = (float)currentDateTime.Day;
            float monthOfYear = (float)currentDateTime.Month;
            float country_of_transaction = viewModel.Order.CountryOfTransaction.Equals("United Kingdom", StringComparison.OrdinalIgnoreCase) ? 1f : 0f;
            float shipping_address = viewModel.Order.ShippingAddress.Equals("United Kingdom", StringComparison.OrdinalIgnoreCase) ? 1f : 0f;

            var input = new List<float> { age, customerId, transactionId, time, amount, dayOfMonth, monthOfYear, country_of_transaction, shipping_address };
            var inputTensor = new DenseTensor<float>(input.ToArray(), new[] { 1, input.Count });

            var inputs = new List<NamedOnnxValue>
    {
        NamedOnnxValue.CreateFromTensor("float_type", inputTensor)
    };

            // Running the ONNX model and handling the prediction
            using (var results = _session.Run(inputs))
            {
                var prediction = results.FirstOrDefault(item => item.Name == "output_label")?.AsTensor<long>().ToArray();
                if (prediction != null && prediction.Length > 0)
                {
                    var fraudStatus = class_type_dict.GetValueOrDefault((int)prediction[0], "Unknown");
                    ViewBag.Prediction = fraudStatus;
                    // viewModel.Order. = fraudStatus; // 

                    // Clear the cart after a successful checkout and before redirecting
                    cart.Clear();
                    SaveCart(cart);

                // Assuming 'Date' is stored as a string. You might need to adjust the format.
                viewModel.Order.Date = currentDateTime.ToString("MM/dd/yyyy");

                // If 'Time' is intended to store hours and minutes, adjust accordingly.
                // This example simply stores the hour for illustration.
                viewModel.Order.Time = currentDateTime.Hour;

                // Set the day of the week
                viewModel.Order.DayOfWeek = currentDateTime.ToString("ddd");

                // Add Order entity to DbSet<Order>
                _context.Orders.Add(viewModel.Order);
                // Add Customer entity to DbSet<Customer>
                _context.Customers.Add(viewModel.Customer);

                // Save changes asynchronously to the database
                _context.SaveChangesAsync();
            }
            return View(viewModel);
        }
    }
}
