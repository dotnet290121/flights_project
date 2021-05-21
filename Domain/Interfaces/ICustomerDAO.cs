using Domain.Entities;

namespace Domain.Interfaces
{
    public interface ICustomerDAO:IBasicDB<Customer>
    {
        Customer GetCustomerByUserId(long user_id);
        Customer GetCustomerByUsername(string username);
        Customer GetCustomerByPhone(string phone);
    }
}
