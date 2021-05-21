using Npgsql;


namespace DAL.ExtentionMethods
{
    public static class NpgsqlDataReaderExtentions
    {
        /// <summary>
        /// Extention method to NpgsqlDataReader. Get string safely from the DB. If the value is null will return null.
        /// </summary>
        /// <param name="reader">The NpgsqlDataReader that is used to read from DB</param>
        /// <param name="colIndex">The index of the desired column</param>
        /// <returns>string if there is a value. If not returns null</returns>
        public static string GetSafeString(this NpgsqlDataReader reader, int colIndex)
        {
            if (!reader.IsDBNull(colIndex))//Check if the value is not null
                return reader.GetString(colIndex);//Return the value
            return null;
        }
    }
}
