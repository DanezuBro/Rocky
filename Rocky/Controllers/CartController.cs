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
        private readonly IOrderHeaderRepository _orderHRepo;
        private readonly IOrderDetailRepository _orderDRepo;

        [BindProperty]
        public ProductUserVM ProductUserVM { get; set; }

        public CartController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment,
                                            IEmailSender emailSender, IProductRepository prodRepo,
                                            IApplicationUserRepository appUserRepo,
                                            IInquiryHeaderRepository inqHRepo, IInquiryDetailRepository inqDRepo,
                                            IOrderHeaderRepository orderHRepo, IOrderDetailRepository orderDRepo )
        {
            _webHostEnvironment = webHostEnvironment;   
            _emailSender = emailSender;
            _prodRepo = prodRepo;   
            _appUserRepo = appUserRepo;
            _inqHRepo = inqHRepo;
            _inqDRepo = inqDRepo;
            _orderHRepo = orderHRepo;
            _orderDRepo = orderDRepo;
        }
        public IActionResult Index()
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if(HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart)!=null && HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart).Count()>0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }

            List<int> prodInCart = shoppingCartList.Select(x => x.ProductId).ToList();
            IEnumerable<Product> prodListTemp = _prodRepo.GetAll(p => prodInCart.Contains(p.Id));
            IList<Product> prodList  = new List<Product>();

            foreach (var cartObj in shoppingCartList)
            {
                Product prodTemp = prodListTemp.FirstOrDefault(p=> p.Id==cartObj.ProductId);
                prodTemp.TempSqFt = cartObj.SqFt;
                prodList.Add(prodTemp);
            }

            return View(prodList);
        }


        [HttpPost,ActionName("Index")]
        [ValidateAntiForgeryToken]
        public IActionResult IndexPost(List<Product> prodList)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            foreach (Product prod in prodList)
            {
                shoppingCartList.Add(new ShoppingCart { ProductId = prod.Id, SqFt = prod.TempSqFt });
            }
            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
            return RedirectToAction(nameof(Summary));
        }


        public IActionResult Summary()
        {
            ApplicationUser applicationUser;
            if (User.IsInRole(WC.AdminRole))
            {
                if (HttpContext.Session.Get<int>(WC.SessionInquiryId) != 0)
                {
                    // cart has been loaded using an inquiry
                    InquiryHeader inquiryHeader = _inqHRepo.FirstOrDefault(i => i.Id == HttpContext.Session.Get<int>(WC.SessionInquiryId));

                    applicationUser = new ApplicationUser()
                    {
                        FullName = inquiryHeader.FullName,
                        Email = inquiryHeader.Email,
                        PhoneNumber = inquiryHeader.PhoneNumber
                    };

                }
                else {
                    applicationUser = new ApplicationUser();
                }
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                // sau alta varianta pentru a lua userId
                //var userId = User.FindFirstValue(ClaimTypes.Name);
                applicationUser = _appUserRepo.FirstOrDefault(a => a.Id == claim.Value);
            };

            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart) != null && HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }

            //List<int> prodInCart = shoppingCartList.Select(x => x.ProductId).ToList();
            //List<Product> prodList = _prodRepo.GetAll(p => prodInCart.Contains(p.Id)).ToList();
            List<Product> prodList = new List<Product>();
            foreach ( ShoppingCart sc in shoppingCartList)
            {
                Product prod = _prodRepo.FirstOrDefault(p=>p.Id==sc.ProductId);
                prod.TempSqFt = sc.SqFt;
                prodList.Add(prod);
            }

            ProductUserVM = new ProductUserVM {

                ApplicationUser = applicationUser,
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

            if (User.IsInRole(WC.AdminRole))
            {
                //we need to create an order
                //var orderTotal = 0.0;
                //foreach (var product in productUserVM.ProductList)
                //{
                //    orderTotal += product.Price * product.TempSqFt;
                //}
                OrderHeader orderHeader = new OrderHeader()
                {
                    CreatedByUserId = claim.Value,
                    FinalOrderTotal = productUserVM.ProductList.Sum(x=>x.Price*x.TempSqFt),// orderTotal,
                    City = productUserVM.ApplicationUser.City,
                    StreetAddress = productUserVM.ApplicationUser.StreetAddress,
                    State = productUserVM.ApplicationUser.State,
                    PostalCode = productUserVM.ApplicationUser.PostalCode,
                    FullName = productUserVM.ApplicationUser.FullName,
                    Email  = productUserVM.ApplicationUser.Email,
                    PhoneNumber = productUserVM.ApplicationUser.PhoneNumber,
                    OrderDate = DateTime.Now,
                    OrderStatus = WC.StatusPending
                };
                _orderHRepo.Add(orderHeader);
                _orderHRepo.Save();

                foreach (var product in productUserVM.ProductList)
                {
                    OrderDetail orderDetail = new OrderDetail() 
                    {
                    OrderHeaderId = orderHeader.Id,
                    PricePerSqFt = product.Price,
                    Sqft = product.TempSqFt,
                    ProductId = product.Id
                    };
                    _orderDRepo.Add(orderDetail);   
                }
                _orderDRepo.Save();
                TempData[WC.Success] = "Order submitted successfully.";
                return RedirectToAction(nameof(InquiryConfirmation), orderHeader);

            }
            else
            {
                // we need to create an inquiry
                var pathToTemplate = _webHostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString() + "templates" + Path.DirectorySeparatorChar.ToString() + "Inquiry.html";
                var subject = "New Inquiry";
                var HtmlBody = "";

                using (StreamReader sr = System.IO.File.OpenText(pathToTemplate))
                {
                    HtmlBody = sr.ReadToEnd();
                }
                StringBuilder productListSB = new StringBuilder();
                var orderTotal = 0.0;
                foreach (var product in productUserVM.ProductList)
                {
                    var prodTotal = product.Price * product.TempSqFt;
                    productListSB.Append($" - Name : {product.Name} <span style='font-size=14px;'>(${product.Price} x {product.TempSqFt} = ${prodTotal}) </span><br/>");
                    orderTotal += prodTotal;
                }
                productListSB.Append($"<span style='font-size=14px;'>(Order total : ${orderTotal}) </span>");
                string messageBody = string.Format(HtmlBody,
                                                                            productUserVM.ApplicationUser.FullName,
                                                                            productUserVM.ApplicationUser.Email,
                                                                            productUserVM.ApplicationUser.PhoneNumber,
                                                                            productListSB.ToString());

                //trimiti email la admin pentru a vedea/pregati comanda
                await _emailSender.SendEmailAsync(WC.EmailAdmin, subject, messageBody);

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
                        ProductId = product.Id,
                        SqFt = product.TempSqFt
                    };
                    _inqDRepo.Add(inquiryDetail);
                }
                _inqDRepo.Save();
                TempData[WC.Success] = "Inquiry submitted successfully.";
            return RedirectToAction(nameof(InquiryConfirmation));
            }

        }

        public IActionResult InquiryConfirmation(OrderHeader orderHeader)
        {
            //OrderHeader orderHeader = _orderHRepo.FirstOrDefault(h => h.Id == id);
            HttpContext.Session.Clear();
            //ViewBag.Request = request;
            return View(orderHeader);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateCart(List<Product> prodList)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            foreach (Product prod in prodList)
            {
                shoppingCartList.Add(new ShoppingCart {ProductId = prod.Id, SqFt = prod.TempSqFt });
            }
            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
            return RedirectToAction(nameof(Index));
        }


    }
}
