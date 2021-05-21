using System.Collections.Generic;

namespace Domain.Interfaces
{
    public interface IBasicDB<T> where T : IPoco, new()
    {
        T Get(long id);
        IList<T> GetAll();
        long Add(T t);
        void Remove(T t);
        void Update(T t);
        //List<T> Run_Generic_SP(string sp_name, object dataHolder, bool ignore_user=false);
    }
}
