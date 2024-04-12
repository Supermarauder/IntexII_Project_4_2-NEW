using IntexII_Project_4_2.Data;
using IntexII_Project_4_2.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace IntexII_Project_4_2.Controllers
{
    public abstract class BaseController : Controller
    {
        protected Cart GetCart()
        {
            return HttpContext.Session.GetJson<Cart>("Cart") ?? new Cart();
        }

        protected void SaveCart(Cart cart)
        {
            HttpContext.Session.SetJson("Cart", cart);
        }
    }
}