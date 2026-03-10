
namespace Systems.Persistence
{
    public interface IDataService
    {
        void Save<T>(T data, bool overwrite = true) where T : ISaveData;
        T Load<T>(string name) where T : ISaveData;
    }
}