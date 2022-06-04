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
        public string StreetAddress { get; set; }
        [NotMapped]
        public string City { get; set; }
        [NotMapped]
        public string State { get; set; }
        [NotMapped]
        [Display(Name ="Postal code")]
        public string PostalCode { get; set; }
    }
}
