using BL.LoginService;

namespace BL
{
    public interface IFlightCenterSystem
    {
        T GetFacade<T>() where T : FacadeBase, new();
         bool TryLogin(string user_name, string password, out ILoginToken token, out FacadeBase facade);
    }
}