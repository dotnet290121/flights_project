using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.ExtentionMethods
{
    public static class ObjectExtenstions
    {
        public static string GenerateString(this object props_holder)
        {
            var props = props_holder.GetType().GetProperties();
            string props_string = "";
            foreach (var prop in props)
                props_string += $"{prop.Name}:{prop.GetValue(props_holder)}, ";

            if (!string.IsNullOrEmpty(props_string))
                props_string = props_string.Remove(props_string.Length - 2);

            return props_string;
        }
    }
}
