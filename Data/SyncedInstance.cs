using MovementCompanyEnhanced.Core;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.Netcode;

namespace MovementCompanyEnhanced.Data {
    [Serializable]
    public class SyncedInstance<T> {
        public static T Instance { get; internal set; }
        public static bool Synced { get; internal set; }

        internal static CustomMessagingManager MessageManager => NetworkManager.Singleton.CustomMessagingManager;

        internal static bool IsClient => NetworkManager.Singleton.IsClient;

        internal static bool IsHost => NetworkManager.Singleton.IsHost;

        internal static byte[] SerializeToBytes(T val) {
            BinaryFormatter bf = new();
            using MemoryStream stream = new();

            try {
                bf.Serialize(stream, val);
                return stream.ToArray();
            }
            catch (Exception e) {
                Plugin.Logger.LogError($"Error serializing instance: {e}");
                return null;
            }
        }

        internal static T DeserializeFromBytes(byte[] data) {
            BinaryFormatter bf = new();
            using MemoryStream stream = new(data);

            try {
                return (T) bf.Deserialize(stream);
            } catch (Exception e) {
                Plugin.Logger.LogError($"Error deserializing instance: {e}");
                return default;
            }
        }

        internal static void UpdateInstance(byte[] data) {
            Instance = DeserializeFromBytes(data);
            Synced = true;
        }
    }
}
