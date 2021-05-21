using System;
using System.Runtime.Serialization;

namespace BL.Exceptions
{
    [Serializable]
    public class NotAllowedAirlineActionException : Exception
    {
        public NotAllowedAirlineActionException()
        {
        }

        public NotAllowedAirlineActionException(string message) : base(message)
        {
        }

        public NotAllowedAirlineActionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NotAllowedAirlineActionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
