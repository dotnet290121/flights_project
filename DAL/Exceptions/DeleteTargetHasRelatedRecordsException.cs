using System;
using System.Runtime.Serialization;

namespace DAL.Exceptions
{
    [Serializable]
    public class DeleteTargetHasRelatedRecordsException : Exception
    {
        const string message = "Related record not exists in Db";

        public string TableName { get; set; }
        public string Statement { get; set; }
        public string ConstraintName { get; set; }

        public DeleteTargetHasRelatedRecordsException()
        {
        }

        public DeleteTargetHasRelatedRecordsException(string message) : base(message)
        {
        }

        public DeleteTargetHasRelatedRecordsException(string message, Exception innerException) : base(message, innerException)
        {
        }
        public DeleteTargetHasRelatedRecordsException(Exception innerException, string tableName, string statement, string constraintName, string message = message) : base(message, innerException)
        {
            TableName = tableName;
            Statement = statement;
            ConstraintName = constraintName;
        }

        protected DeleteTargetHasRelatedRecordsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}