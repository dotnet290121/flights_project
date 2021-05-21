using System.Collections;
using System.Text;

namespace Domain.ExtentionMethods
{
    public static class IEnumerableExtenstions
    {
        public static string BuildString(this IEnumerable enumerable)
        {
            StringBuilder result_string_builder = new StringBuilder();
            foreach (var item in enumerable)
                result_string_builder.Append($"{item}, ");

            if (result_string_builder.Length == 0)
                return "Empty collection";

            return result_string_builder.ToString();
        }
    }
}
