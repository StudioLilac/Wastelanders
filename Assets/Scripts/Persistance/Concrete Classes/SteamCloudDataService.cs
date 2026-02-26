using System.IO;
using Steamworks;
using UnityEngine;

namespace Systems.Persistence
{
    public class SteamCloudDataService : IDataService
    {
        private readonly ISerializer serializer;
        private const string EXTENSION = ".json";

        public SteamCloudDataService(ISerializer serializer)
        {
            this.serializer = serializer;
        }

        private string GetPath(string name) => name + EXTENSION;

        public void Save<T>(T data, bool overwrite = true) where T : ISaveData
        {
            if (!SteamManager.Initialized)
            {
                Debug.LogWarning("[SteamCloudDataService] Steam is not initialized. Cannot save.");
                return;
            }
#if STEAMWORKS_NET
            string json = serializer.Serialize(data);
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);
            string path = GetPath(data.SaveName);

            bool result = SteamRemoteStorage.FileWrite(path, bytes, bytes.Length);

            if (!result)
            {
                Debug.LogError($"[SteamCloudDataService] Failed to write to Steam Cloud: {path}");
            }
            else
            {
                Debug.Log($"[SteamCloudDataService] Saved to Steam Cloud: {path}");
            }
#endif
        }

        public T Load<T>(string name) where T : ISaveData
        {
            if (!SteamManager.Initialized)
            {
                throw new IOException("Steam is not initialized.");
            }

            T gamedData = default(T);
#if STEAMWORKS_NET
            string path = GetPath(name);

            if (!SteamRemoteStorage.FileExists(path))
            {
                throw new FileNotFoundException($"Steam Cloud file '{path}' does not exist.");
            }

            int size = SteamRemoteStorage.GetFileSize(path);
            byte[] buffer = new byte[size];

            int read = SteamRemoteStorage.FileRead(path, buffer, size);

            if (read == 0 && size > 0)
            {
                throw new IOException($"Steamworks failed to read the file '{path}'. File may be locked or corrupted.");
            }

            string json = System.Text.Encoding.UTF8.GetString(buffer, 0, read);
            gamedData = serializer.Deserialize<T>(json);
#endif
            return gamedData;
        }
    }
}