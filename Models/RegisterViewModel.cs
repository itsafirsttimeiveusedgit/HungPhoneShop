using System.ComponentModel.DataAnnotations;

namespace HungPhoneShop.Models
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "Full Name")]
        public string? FullName { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
    }
}