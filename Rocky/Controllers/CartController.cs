using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rocky.Data;
using Rocky.Models;
using Rocky.Models.ViewModels;
using Rocky.Utility;
using System.Security.Claims;

namespace Rocky.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;

        [BindProperty]
        public ProductUserVM ProductUserVM { get; set; }

        public CartController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if(HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart)!=null && HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart).Count()>0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }

            List<int> prodInCart = shoppingCartList.Select(x => x.ProductId).ToList();
            List<Product> prodList = _db.Product.Where(p => prodInCart.Contains(p.Id)).ToList();
            return View(prodList);
        }


        [HttpPost,ActionName("Index")]
        [ValidateAntiForgeryToken]
        public IActionResult IndexPost()
        {
            return RedirectToAction(nameof(Summary));
        }


        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            // sau alta varianta pentru a lua userId
            //var userId = User.FindFirstValue(ClaimTypes.Name);

            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart) != null && HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }

            List<int> prodInCart = shoppingCartList.Select(x => x.ProductId).ToList();
            List<Product> prodList = _db.Product.Where(p => prodInCart.Contains(p.Id)).ToList();

            ProductUserVM = new ProductUserVM {

                ApplicationUser = _db.ApplicationUser.FirstOrDefault(a=>a.Id==claim.Value),
                ProductList = prodList
            };
            return View(ProductUserVM);
        }


        public IActionResult Remove(int id)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart) != null && HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart).Count > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }

            shoppingCartList.Remove(shoppingCartList.FirstOrDefault(p => p.ProductId == id));
            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
            return RedirectToAction(nameof(Index));
        }



    }
}
