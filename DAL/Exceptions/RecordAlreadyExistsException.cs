using System;
using System.Runtime.Serialization;

namespace DAL.Exceptions
{
    [Serializable]
    public class RecordAlreadyExistsException : Exception
    {
        const string message = "Record already exists in Db";

        public string TableName { get; set; }
        public string Statement { get; set; }
        public string ConstraintName { get; set; }

        public RecordAlreadyExistsException()
        {
        }

        public RecordAlreadyExistsException(string message) : base(message)
        {
        }

        public RecordAlreadyExistsException(string message, Exception innerException) : base(message, innerException)
        {
        }
        public RecordAlreadyExistsException(Exception innerException, string tableName, string statement, string constraintName, string message = message) : base(message, innerException)
        {
            TableName = tableName;
            Statement = statement;
            ConstraintName = constraintName;
        }

        protected RecordAlreadyExistsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}