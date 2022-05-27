using Rocky_Models;

namespace Rocky_Models.ViewModels
{
    public class ProductUserVM
    {
        public ProductUserVM()
        {
            //ApplicationUser = new ApplicationUser(); 
            ProductList = new List<Product>();
        }
        public ApplicationUser ApplicationUser { get; set; }
        public List<Product> ProductList { get; set; }
    }
}
