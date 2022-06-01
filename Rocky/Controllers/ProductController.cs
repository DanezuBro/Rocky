using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rocky_DataAccess.Data;
using Rocky_DataAccess.Repository.IRepository;
using Rocky_Models;
using Rocky_Models.ViewModels;
using Rocky_Utility;

namespace Rocky.Controllers
{
    [Authorize(Roles =WC.AdminRole)]
    public class ProductController : Controller
    {
        private readonly IProductRepository _prodRepo;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IProductRepository prodRepo, IWebHostEnvironment webHostEnvironment)
        {
            _prodRepo = prodRepo;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> productsList = _prodRepo.GetAll(includeProperties: "Category,ApplicationType");//Include(c=>c.Category).Include(a=>a.ApplicationType).ToList();
            return View(productsList);
        }

        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new ProductVM()
            {
                Product = new Product(),
                CategorySelectList = _prodRepo.GetAllDropdownList(WC.CategoryName),
                ApplicationTypeSelectList = _prodRepo.GetAllDropdownList(WC.ApplicationTypeName)
            };

            if (id == null) {
                // this is for create
                return View(productVM);
            }
            else
            {
                // this is for edit
                productVM.Product = _prodRepo.FirstOrDefault(p => p.Id == id, includeProperties: "Category,ApplicationType");//_db.Product.Include(c => c.Category).Include(a => a.ApplicationType).FirstOrDefault(p=>p.Id==id);
                if (productVM.Product == null)
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
                    _prodRepo.Add(productVM.Product);
                    TempData[WC.Success] = "Product created successfully.";
                }
                else
                {
                    //updating product
                    var objFromDb = _prodRepo.FirstOrDefault(p => p.Id == productVM.Product.Id, isTracking:false);//_db.Product.AsNoTracking().FirstOrDefault(p => p.Id == productVM.Product.Id);
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
                    _prodRepo.Update(productVM.Product);
                    TempData[WC.Success] = "Product updated successfully.";
                }

                _prodRepo.Save();

                return RedirectToAction("Index");
            }
            productVM.CategorySelectList = _prodRepo.GetAllDropdownList(WC.CategoryName); //_db.Category.Select(c => new SelectListItem
            //{
            //    Text = c.Name,
            //    Value = c.Id.ToString()
            //});
            productVM.ApplicationTypeSelectList = _prodRepo.GetAllDropdownList(WC.ApplicationTypeName); //_db.ApplicationType.Select(a => new SelectListItem { Text = a.Name, Value = a.Id.ToString() });
            TempData[WC.Error] = "Product was not created.";
            return View(productVM);
        }


        public IActionResult Delete(int id)
        {
            Product product = _prodRepo.FirstOrDefault(p => p.Id == id, includeProperties: "Category,ApplicationType");//_db.Product.Include(c => c.Category).Include(a=>a.ApplicationType).FirstOrDefault(p=>p.Id==id);
            
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
        public IActionResult DeletePost(int? id)
        {
            Product product = _prodRepo.Find(id.GetValueOrDefault());
            if (product == null)
            {
                TempData[WC.Error] = "Product was not deleted.";
                return NotFound();
            }

            var webRootPath = _webHostEnvironment.WebRootPath;

            var oldFile = Path.Combine(webRootPath + WC.ImagePath, product.Image);
            if (System.IO.File.Exists(oldFile))
            {
                System.IO.File.Delete(oldFile);
            }
            _prodRepo.Remove(product);
            _prodRepo.Save();
            TempData[WC.Success] = "Product was deleted.";
            return RedirectToAction("Index");
        }

    }
}
