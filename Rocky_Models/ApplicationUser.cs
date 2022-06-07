using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rocky_Models
{
    public class ApplicationUser : IdentityUser
    {

        public string FullName { get; set; }
        [NotMapped]
        [Display(Name = "Street address")]
        [Required]
        public string StreetAddress { get; set; }
        [NotMapped]
        [Required]
        public string City { get; set; }
        [NotMapped]
        [Required]
        public string State { get; set; }
        [NotMapped]
        [Display(Name ="Postal code")]
        [Required]
        public string PostalCode { get; set; }
    }
}
