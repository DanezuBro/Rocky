using Microsoft.AspNetCore.Mvc.Rendering;
using Rocky_DataAccess.Data;
using Rocky_DataAccess.Repository.IRepository;
using Rocky_Models;
using Rocky_Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rocky_DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _db;

        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public IEnumerable<SelectListItem> GetAllDropdownList(string obj)
        {
            if(obj==WC.CategoryName)
            {
                return _db.Category.Select(c => new SelectListItem { Text = c.Name, Value = c.Id.ToString() });
            }
            if (obj == WC.ApplicationTypeName)
            {
                return _db.ApplicationType.Select(a => new SelectListItem { Text = a.Name, Value = a.Id.ToString() });
            }
            return null;
         }

        public void Update(Product obj)
        {
            _db.Product.Update(obj);
        }
    }
}
