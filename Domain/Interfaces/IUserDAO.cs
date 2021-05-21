using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IUserDAO : IBasicDB<User>
    {
        User GetUserByUserNameAndPassword(string username, string password);
        User GetUserByUserName(string username);
    }
}
