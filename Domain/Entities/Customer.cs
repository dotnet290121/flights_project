using Domain.Interfaces;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Customer : IPoco, IUser
    {
        [Column("customer_id")]
        public long Id { get; set; }
        [Column("first_name")]
        public string FirstName { get; set; }
        [Column("last_name")]
        public string LastName { get; set; }
        public string Address { get; set; }
        [Column("phone_number")]
        public string PhoneNumber { get; set; }
        [Column("credit_card_number")]
        public string CreditCardNumber { get; set; }
        public User User { get; set; }

        public Customer()
        {

        }

        public Customer(string firstName, string lastName, string address, string phoneNumber, string creditCardNumber, User user, long id = 0)
        {
            FirstName = firstName;
            LastName = lastName;
            Address = address;
            PhoneNumber = phoneNumber;
            CreditCardNumber = creditCardNumber;
            User = user;
            Id = id;
        }

        public static bool operator ==(Customer customer1, Customer customer2)
        {
            if (ReferenceEquals(customer1, null) && ReferenceEquals(customer2, null))
                return true;
            if (ReferenceEquals(customer1, null) || ReferenceEquals(customer2, null))
                return false;

            return customer1.Id == customer2.Id;
        }
        public static bool operator !=(Customer customer1, Customer customer2)
        {
            return !(customer1 == customer2);
        }
        public override bool Equals(object obj)
        {
            Customer customer = obj as Customer;
            return this == customer;
        }

        public override int GetHashCode()
        {
            return (int)this.Id;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
