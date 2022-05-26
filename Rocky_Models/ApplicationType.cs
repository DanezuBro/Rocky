using System.ComponentModel.DataAnnotations;

namespace Rocky_Models
{
    public class ApplicationType
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MinLength(1, ErrorMessage ="Minim length required for Name")]
        public string Name { get; set; }
    }
}
