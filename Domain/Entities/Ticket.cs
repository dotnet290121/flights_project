using Domain.Interfaces;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Ticket : IPoco
    {
        [Column("ticket_id")]
        public long Id { get; set; }
        public Flight Flight { get; set; }
        public Customer Customer { get; set; }

        public Ticket()
        {

        }

        public Ticket(Flight flight, Customer customer, long id = 0)
        {
            Flight = flight;
            Customer = customer;
            Id = id;
        }

        public static bool operator ==(Ticket ticket1, Ticket ticket2)
        {
            if (ReferenceEquals(ticket1, null) && ReferenceEquals(ticket2, null))
                return true;
            if (ReferenceEquals(ticket1, null) || ReferenceEquals(ticket2, null))
                return false;

            return ticket1.Id == ticket2.Id;
        }
        public static bool operator !=(Ticket ticket1, Ticket ticket2)
        {
            return !(ticket1 == ticket2);
        }
        public override bool Equals(object obj)
        {
            Ticket ticket = obj as Ticket;
            return this == ticket;
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
