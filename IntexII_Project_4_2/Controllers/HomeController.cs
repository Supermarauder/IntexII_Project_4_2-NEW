using IntexII_Project_4_2.Data;
using IntexII_Project_4_2.Models.ViewModels;
using IntexII_Project_4_2.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ML.OnnxRuntime;
using IntexII_Project_4_2.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntexII_Project_4_2.Controllers
{
    public class HomeController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private IIntexProjectRepository _repo;
        private InferenceSession _session;
        public string _onnxModelPath;

        public HomeController(IIntexProjectRepository temp, IWebHostEnvironment hostEnvironment, UserManager<ApplicationUser> userManager)
        {
            _repo = temp;
            _onnxModelPath = System.IO.Path.Combine(hostEnvironment.WebRootPath, "model.onnx");
            _session = new InferenceSession(_onnxModelPath);
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            ApplicationUser currentUser = await _userManager.GetUserAsync(User);
            List<Product> recommendations;

            if (currentUser != null)
            {
                var customerRecommendation = await _repo.CustomerRecommendations.FirstOrDefaultAsync(cr => cr.CustomerID == currentUser.CustomerId);

                if (customerRecommendation != null)
                {
                    var recommendationIds = new List<int>
                    {
                        customerRecommendation.Recommendation1,
                        customerRecommendation.Recommendation2,
                        customerRecommendation.Recommendation3,
                        customerRecommendation.Recommendation4,
                        customerRecommendation.Recommendation5
                    };

                    recommendations = await _repo.Products.Where(p => recommendationIds.Contains(p.ProductId)).ToListAsync();
                }
                else
                {
                    recommendations = GetTopRecommendations();
                }
            }
            else
            {
                recommendations = GetTopRecommendations();
            }

            var viewModel = new IndexViewModel
            {
                Recommendations = recommendations
            };

            return View(viewModel);
        }

        private List<Product> GetTopRecommendations()
        {
            var topRecommendationIds = _repo.TopRecommendations.Select(tr => tr.ProductID).ToList();
            return _repo.Products.Where(p => topRecommendationIds.Contains(p.ProductId)).ToList();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult AddToCart(int productId, int quantity)
        {
            Product product = _repo.Products.FirstOrDefault(p => p.ProductId == productId);
            if (product == null)
            {
                return NotFound();
            }

            Cart cart = HttpContext.Session.GetJson<Cart>("cart") ?? new Cart();
            cart.AddItem(product, quantity);
            HttpContext.Session.SetJson("cart", cart);

            return RedirectToPage("/Cart");
        }

        public IActionResult CartSummary()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult ProductDetail(int id)
        {
            Product product = _repo.Products.FirstOrDefault(p => p.ProductId == id);
            if (product == null)
            {
                return NotFound(); // Or any other error handling
            }

            var recommendation = _repo.ItemRecommendations.FirstOrDefault(r => r.ProductID == id);
            List<Product> recommendedProducts = new List<Product>();
            if (recommendation != null)
            {
                recommendedProducts.Add(_repo.Products.FirstOrDefault(p => p.ProductId == recommendation.Recommendation1));
                recommendedProducts.Add(_repo.Products.FirstOrDefault(p => p.ProductId == recommendation.Recommendation2));
                recommendedProducts.Add(_repo.Products.FirstOrDefault(p => p.ProductId == recommendation.Recommendation3));
                recommendedProducts.Add(_repo.Products.FirstOrDefault(p => p.ProductId == recommendation.Recommendation4));
                recommendedProducts.Add(_repo.Products.FirstOrDefault(p => p.ProductId == recommendation.Recommendation5));
            }

            var viewModel = new ProductDetailViewModel
            {
                Product = product,
                Recommendations = recommendedProducts
            };

            return View(viewModel);
        }

        public IActionResult Register()
        {
            return View();
        }

        public IActionResult ViewProducts(int pageNum, string[] categories, string[] colors, int pageSize = 5)
        {
            pageNum = Math.Max(1, pageNum); // Ensure pageNum is at least 1

            IQueryable<Product> query = _repo.Products.AsQueryable();

            // Apply category filters if provided
            if (categories != null && categories.Length > 0)
            {
                query = query.Where(p => categories.Any(cat => p.Category.Contains(cat)));
            }

            // Apply color filters if provided
            if (colors != null && colors.Length > 0)
            {
                query = query.Where(p => colors.Contains(p.PrimaryColor));
            }

            int totalItems = query.Count();
            List<Product> filteredProducts = query.OrderBy(p => p.Name).Skip((pageNum - 1) * pageSize).Take(pageSize).ToList();

            var productList = new ProductListViewModel
            {
                Products = filteredProducts,
                PaginationInfo = new PaginationInfo
                {
                    CurrentPage = pageNum,
                    ItemsPerPage = pageSize, // Use the pageSize parameter
                    TotalItems = totalItems
                }
            };

            return View(productList);
        }
    }
}