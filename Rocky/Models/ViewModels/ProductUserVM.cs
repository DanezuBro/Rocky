﻿namespace Rocky.Models.ViewModels
{
    public class ProductUserVM
    {
        public ProductUserVM()
        {
            //ApplicationUser = new ApplicationUser(); 
            ProductList = new List<Product>();
        }
        public ApplicationUser ApplicationUser { get; set; }
        public IEnumerable<Product> ProductList { get; set; }
    }
}
