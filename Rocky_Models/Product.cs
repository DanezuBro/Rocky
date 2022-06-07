using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rocky_Models
{
    public class Product
    {
        public Product()
        {
            TempSqFt = 1;
        }
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [ValidateNever]
        public string ShortDesc { get; set; }
        [ValidateNever]
        public string Description { get; set; }

        [Range(1, int.MaxValue)]
        [Required]
        public double Price { get; set; }
        [ValidateNever]
        public string Image { get; set; }

        [Display(Name = "Category Name")]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        [ValidateNever]
        public Category Category { get; set; }

        [Display(Name = "Application Type Name")]
        public int ApplicationTypeId { get; set; }

        [ForeignKey("ApplicationTypeId")]
        [ValidateNever]
        public ApplicationType ApplicationType { get; set; }

        [NotMapped]
        [Range(1,10000)]
        public int TempSqFt { get; set; }

    }
}
