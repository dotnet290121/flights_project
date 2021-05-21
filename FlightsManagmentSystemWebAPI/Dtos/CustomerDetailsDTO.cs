
namespace FlightsManagmentSystemWebAPI.Dtos
{
    public class CustomerDetailsDTO: IDetailsDTO
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public UserDetailsDTO User { get; set; }
    }
}
