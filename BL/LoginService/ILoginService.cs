
using Domain.Entities;
using Domain.Interfaces;

namespace BL.LoginService
{
    public interface ILoginService
    {
        bool TryLogin(string userName, string password, out ILoginToken token, out FacadeBase facade);
        //bool IsValidUserNameAndPassword(string username, string password, out UserRoles role);
        //User GetUser(string username);
    }
}
