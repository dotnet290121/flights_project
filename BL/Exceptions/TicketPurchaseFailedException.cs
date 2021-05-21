using System;
using System.Runtime.Serialization;

namespace BL.Exceptions
{
    public enum PurchaseFailReason
    {
        Flight_Took_Off=1,
        No_Tickets_Left=2
    }

    [Serializable]
    public class TicketPurchaseFailedException : Exception
    {
        public PurchaseFailReason PurchaseFailReason { get; set; }

        public TicketPurchaseFailedException()
        {
        }

        public TicketPurchaseFailedException(string message) : base(message)
        {
        }

        public TicketPurchaseFailedException(string message, PurchaseFailReason purchaseFailReason) : base(message)
        {
            PurchaseFailReason = purchaseFailReason;
        }

        public TicketPurchaseFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected TicketPurchaseFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}