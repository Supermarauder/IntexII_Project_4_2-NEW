using IntexII_Project_4_2.Data;
using IntexII_Project_4_2.Infrastructure;
using IntexII_Project_4_2.Models;
using IntexII_Project_4_2.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Globalization;

namespace IntexII_Project_4_2.Controllers
{
    public class AuthUserController : BaseController
    {

        private readonly IIntexProjectRepository _repo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly InferenceSession _session;
        public readonly string _onnxModelPath;

        public AuthUserController(IIntexProjectRepository repo, IWebHostEnvironment hostEnvironment)
        {
            _repo = repo;
            _onnxModelPath = System.IO.Path.Combine(hostEnvironment.WebRootPath, "Final_Model.onnx");
            _session = new InferenceSession(_onnxModelPath);
        }

        public IActionResult Checkout()
        {
            return View();
        }
        [Authorize(Roles = "Member")]
        public IActionResult Confirmation0()
        {
            return View();
            var customerId = HttpContext.Session.GetInt32("CustomerId");

            var viewModel = new OrderPrediction
            {

                Cart = HttpContext.Session.GetJson<Cart>("cart"),
                CustomerId = customerId ?? default
            };

            return View(viewModel);
        }

        [Authorize(Roles = "Member")]
        public IActionResult Confirmation1()

        [HttpPost]
        public IActionResult Checkout(OrderPrediction viewModel)
        {
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
            float age = (float)viewModel.Customer.Age;
            float customerId = (float)viewModel.Order.CustomerId;
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

                    // Clear the cart after a successful checkout and before redirecting
                    cart.Clear();
                    SaveCart(cart);

                    // Determine the redirect action based on the prediction
                    var redirectAction = prediction[0] == 0 ? "Confirmation0" : "Confirmation1";
                    return RedirectToAction(redirectAction);
                }
                else
                {
                    ViewBag.Prediction = "Error: Unable to make a prediction.";
                }
            }

            // Handling cases where prediction was not made
            // Clear the cart and save it
            cart.Clear();
            SaveCart(cart);

            // Redirect to a default confirmation page or an error handling page as appropriate
            return RedirectToAction("Confirmation0");
        }


        public IActionResult Confirmation0()
            {
                return View();
            }
            public IActionResult Confirmation1()
            {
                return View();
            }
            public IActionResult RegistrationSuccessful()
            {
                return View();
            }
        }
    } 

