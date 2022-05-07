using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rocky.Data;
using Rocky.Models;
using Rocky.Models.ViewModels;

namespace Rocky.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> productsList = _db.Product.Include(c=>c.Category).Include(a=>a.ApplicationType).ToList();
            return View(productsList);
        }

        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new ProductVM()
            {
                Product = new Product(),
                CategorySelectList = _db.Category.Select(c => new SelectListItem { Text = c.Name, Value = c.Id.ToString() }),
                ApplicationTypeSelectList = _db.ApplicationType.Select(a=> new SelectListItem { Text = a.Name, Value = a.Id.ToString() })
            };

            if (id == null) {
                // this is for create
                return View(productVM);
            }
            else
            {
                // this is for edit
                productVM.Product = _db.Product.Include(c => c.Category).Include(a => a.ApplicationType).FirstOrDefault(p=>p.Id==id);
                if(productVM.Product == null)
                {
                    return NotFound();
                }
                return View(productVM);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM productVM)
        {

            if (ModelState.IsValid)
            {

                var files = HttpContext.Request.Form.Files;
                var webRootPath = _webHostEnvironment.WebRootPath;

                if (productVM.Product.Id == 0)
                {
                    // creating product
                    string upload = webRootPath + WC.ImagePath;
                    string fileName = Guid.NewGuid().ToString();
                    string extension = Path.GetExtension(files[0].FileName);

                    using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create)) {
                        files[0].CopyTo(fileStream);
                    }

                    productVM.Product.Image = fileName + extension;
                    _db.Product.Add(productVM.Product);

                }
                else
                {
                    //updating product
                    var objFromDb = _db.Product.AsNoTracking().FirstOrDefault(p => p.Id == productVM.Product.Id);
                    if (files.Count > 0)
                    {
                        string upload = webRootPath + WC.ImagePath;
                        string fileName = Guid.NewGuid().ToString();
                        string extension = Path.GetExtension(files[0].FileName);

                        var oldFile = Path.Combine(upload, objFromDb.Image);
                        if (System.IO.File.Exists(oldFile))
                        {
                            System.IO.File.Delete(oldFile);
                        }

                        using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                        {
                            files[0].CopyTo(fileStream);
                        }
                        productVM.Product.Image = fileName + extension;
                    }
                    else
                    {
                        productVM.Product.Image = objFromDb.Image;
                    }
                    _db.Product.Update(productVM.Product);

                }

                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            productVM.CategorySelectList = _db.Category.Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString()
            });
            productVM.ApplicationTypeSelectList = _db.ApplicationType.Select(a => new SelectListItem { Text = a.Name, Value = a.Id.ToString() });
            return View(productVM);
        }


        public IActionResult Delete(int id)
        {
            Product product = _db.Product.Include(c => c.Category).Include(a=>a.ApplicationType).FirstOrDefault(p=>p.Id==id);
            
            if(product!=null)
            {
                return View(product);
            }
            else
            {
                return View();
            }
        }


        [HttpPost,ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int id)
        {
            Product product = _db.Product.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            var webRootPath = _webHostEnvironment.WebRootPath;

            var oldFile = Path.Combine(webRootPath + WC.ImagePath, product.Image);
            if (System.IO.File.Exists(oldFile))
            {
                System.IO.File.Delete(oldFile);
            }
            _db.Product.Remove(product);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

    }
}
