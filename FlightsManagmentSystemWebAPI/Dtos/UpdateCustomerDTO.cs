using System.ComponentModel.DataAnnotations;

namespace FlightsManagmentSystemWebAPI.Dtos
{
    public class UpdateCustomerDTO
    {
        [Range(1, long.MaxValue)]
        public long Id { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 2)]
        public string FirstName { get; set; }
        [Required]
        [StringLength(20, MinimumLength = 2)]
        public string LastName { get; set; }
        [StringLength(100)]
        public string Address { get; set; }
        [Required]
        [Phone]
        public string PhoneNumber { get; set; }
        [CreditCard]
        public string CreditCardNumber { get; set; }
    }
}
