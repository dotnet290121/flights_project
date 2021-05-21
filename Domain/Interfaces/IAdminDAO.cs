using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IAdminDAO : IBasicDB<Administrator>
    {
        Administrator GetAdministratorByUserId(long user_id);
    }
}
