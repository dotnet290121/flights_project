using Domain.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BL.LoginService
{
    public class LoginToken<T> : ILoginToken where T : IUser
    {
        public T User { get; set; }

        public LoginToken(T user)
        {
            User = user;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
