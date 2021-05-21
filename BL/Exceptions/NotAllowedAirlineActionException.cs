using System;
using System.Runtime.Serialization;

namespace BL.Exceptions
{
    [Serializable]
    public class NotAllowedAdminActionException : Exception
    {
        public NotAllowedAdminActionException()
        {
        }

        public NotAllowedAdminActionException(string message) : base(message)
        {
        }

        public NotAllowedAdminActionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NotAllowedAdminActionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
