using System;
using System.Runtime.Serialization;

namespace BL.Exceptions
{
    [Serializable]
    public class WrongCustomerException : Exception
    {
        public WrongCustomerException()
        {
        }

        public WrongCustomerException(string message) : base(message)
        {
        }

        public WrongCustomerException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected WrongCustomerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
