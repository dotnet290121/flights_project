using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public enum TicketStatus
    {
        Redeemed = 0,
        Cancelled_By_Company = 1,
        Cancelled_By_Customer = 2,
        Cancelled_By_Administrator = 3
    }

    public class TicketHistory
    {
        public long Id { get; set; }

        [Column("original_ticket_id")]
        public long OriginalId { get; set; }

        [Column("flight_id")]
        public long FlightId { get; set; }

        [Column("customer_id")]
        public long CustomerId { get; set; }

        [Column("customer_full_name")]
        public string CustomerFullName { get; set; }

        [Column("customer_username")]
        public string CustomerUserName { get; set; }

        [Column("ticket_status")]
        public TicketStatus TicketStatus { get; set; }
    }
}
