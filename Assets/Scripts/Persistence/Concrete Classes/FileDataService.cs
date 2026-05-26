
using System.IO;
using UnityEngine;

namespace Systems.Persistence
{
    public class FileDataService : IDataService
    {
        ISerializer serializer;
        string dataPath;
        string fileExtension;

        public FileDataService(ISerializer serializer, string overridePath = null)
        {
            // Application.persistentDataPath = C:\Users\YOUR_USER_NAME\AppData\LocalLow\S2dio\WastelandersGame
            this.dataPath = string.IsNullOrEmpty(overridePath) ? Application.persistentDataPath : overridePath;
            this.fileExtension = "json";
            this.serializer = serializer;
        }

        string GetPathToFile(string fileName)
        {
            return Path.Combine(dataPath, string.Concat(fileName, ".", fileExtension));
        }

        public void Save<T>(T data, bool overwrite = true) where T: ISaveData
        {
            string fileLocation = GetPathToFile(data.SaveName);

            if (!overwrite && File.Exists(fileLocation))
            {
                throw new IOException($"File '{data.SaveName}.{fileExtension}' already exists and can't be overwritten.");
            }

            Debug.Log("Saving Content to " + Application.persistentDataPath);
            File.WriteAllText(fileLocation, serializer.Serialize(data));
        }

        public T Load<T>(string name) where T: ISaveData
        {
            string fileLocation = GetPathToFile(name);

            if (!File.Exists(fileLocation))
            {
                throw new FileNotFoundException($"File '{name}.{fileExtension}' does not exist in this file");
            }

            return serializer.Deserialize<T>(File.ReadAllText(fileLocation));;
        }
    }
}