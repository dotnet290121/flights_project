using System;
using System.Runtime.Serialization;

namespace DAL.Exceptions
{
    [Serializable]
    public class RelatedRecordNotExistsException : Exception
    {
        const string message = "Related record not exists in Db";

        public string TableName { get; set; }
        public string Statement { get; set; }
        public string ConstraintName { get; set; }

        public RelatedRecordNotExistsException()
        {
        }

        public RelatedRecordNotExistsException(string message) : base(message)
        {
        }

        public RelatedRecordNotExistsException(string message, Exception innerException) : base(message, innerException)
        {
        }
        public RelatedRecordNotExistsException(Exception innerException, string tableName, string statement, string constraintName, string message = message) : base(message, innerException)
        {
            TableName = tableName;
            Statement = statement;
            ConstraintName = constraintName;
        }

        protected RelatedRecordNotExistsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}