namespace Etilize.Data
{
    using System;

    public interface IDataService
    {
        void Delete(int id);
        void Edit(int id);
        void Get(int id);
        void Save();
    }
}

