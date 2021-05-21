using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FlightsManagmentSystemWebAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FutureDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            bool is_date = DateTime.TryParse(value.ToString(), out DateTime datetime);
            return is_date ? datetime > DateTime.Now: false;
        }
    }
}
