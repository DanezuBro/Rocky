using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Rocky_DataAccess.Data;
using Rocky_DataAccess.Repository.IRepository;
using Rocky_Models;
using Rocky_Models.ViewModels;
using Rocky_Utility;
using System.Security.Claims;
using System.Text;

namespace Rocky.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEmailSender _emailSender;
        private readonly IProductRepository _prodRepo;
        private readonly IApplicationUserRepository _appUserRepo;
        private readonly IInquiryHeaderRepository _inqHRepo;
        private readonly IInquiryDetailRepository _inqDRepo;

        [BindProperty]
        public ProductUserVM ProductUserVM { get; set; }

        public CartController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment, IEmailSender emailSender, IProductRepository prodRepo, IApplicationUserRepository appUserRepo, IInquiryHeaderRepository inqHRepo, IInquiryDetailRepository inqDRepo)
        {
            _webHostEnvironment = webHostEnvironment;   
            _emailSender = emailSender;
            _prodRepo = prodRepo;   
            _appUserRepo = appUserRepo;
            _inqHRepo = inqHRepo;
            _inqDRepo = inqDRepo;
        }
        public IActionResult Index()
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if(HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart)!=null && HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart).Count()>0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }

            List<int> prodInCart = shoppingCartList.Select(x => x.ProductId).ToList();
            IEnumerable<Product> prodList = _prodRepo.GetAll(p => prodInCart.Contains(p.Id));
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
            List<Product> prodList = _prodRepo.GetAll(p => prodInCart.Contains(p.Id)).ToList();

            ProductUserVM = new ProductUserVM {

                ApplicationUser = _appUserRepo.FirstOrDefault(a=>a.Id==claim.Value),
                ProductList = prodList
            };
            return View(ProductUserVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]
        public async Task<IActionResult> SummaryPost(ProductUserVM productUserVM)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var pathToTemplate = _webHostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString() + "templates" + Path.DirectorySeparatorChar.ToString() + "Inquiry.html";
            var subject ="New Inquiry";
            var HtmlBody = "";

            using (StreamReader sr = System.IO.File.OpenText(pathToTemplate))
            {
                HtmlBody = sr.ReadToEnd();
            }
            StringBuilder productListSB = new StringBuilder();
            foreach (var product in productUserVM.ProductList)
            {
                productListSB.Append($" - Name : {product.Name} <span style='font-size=14px;'>(ID: {product.Id}) </span><br/>");
            }

            string messageBody = string.Format(HtmlBody,
                                                                        productUserVM.ApplicationUser.FullName,
                                                                        productUserVM.ApplicationUser.Email,
                                                                        productUserVM.ApplicationUser.PhoneNumber,
                                                                        productListSB.ToString());

            //trimiti email la admin pentru a vedea/pregati comanda
            await _emailSender.SendEmailAsync(WC.EmailAdmin,subject, messageBody);

            //trimiti email la client pentru a vedea comanda
            await _emailSender.SendEmailAsync(productUserVM.ApplicationUser.Email, subject, messageBody);

            InquiryHeader inquiryHeader = new InquiryHeader()
            {
                ApplicationUserId = claim.Value,
                FullName = productUserVM.ApplicationUser.FullName,
                Email = productUserVM.ApplicationUser.Email,
                PhoneNumber = productUserVM.ApplicationUser.PhoneNumber,
                InquiryDate = DateTime.Now
            };

            _inqHRepo.Add(inquiryHeader);
            _inqHRepo.Save();

            foreach (var product in productUserVM.ProductList)
            {
                InquiryDetail inquiryDetail = new InquiryDetail()
                {
                    InquiryHeaderId = inquiryHeader.Id,
                    ProductId = product.Id
                };
                _inqDRepo.Add(inquiryDetail);
            }
                _inqDRepo.Save();
            TempData[WC.Success] = "Inquiry submitted successfully.";
            return RedirectToAction(nameof(InquiryConfirmation));
        }

        public IActionResult InquiryConfirmation()
        {
            HttpContext.Session.Clear();
            return View();
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
