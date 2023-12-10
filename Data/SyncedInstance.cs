using MovementCompanyEnhanced.Core;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.Netcode;

namespace MovementCompanyEnhanced.Data {
    [Serializable]
    public class SyncedInstance<T> {
        public static T Instance { get; internal set; }
        public static bool synced { get; internal set; }

        internal static byte[] SerializeToBytes(T val) {
            BinaryFormatter bf = new();
            MemoryStream stream = new();

            try {
                bf.Serialize(stream, val);
            } catch(Exception e) {
                Plugin.Logger.LogError($"Error serializing instance: {e}");
            }

            return stream.ToArray();
        }

        internal static T DeserializeFromBytes(byte[] data) {
            MemoryStream serializationStream = new(data);
            BinaryFormatter binaryFormatter = new();

            return (T) binaryFormatter.Deserialize(serializationStream);
        }

        internal static void UpdateInstance(byte[] data) {
            Instance = DeserializeFromBytes(data);
            synced = true;
        }

        internal static CustomMessagingManager MessageManager() {
            return NetworkManager.Singleton.CustomMessagingManager;
        }

        internal static bool IsClient() {
            return NetworkManager.Singleton.IsClient;
        }

        internal static bool IsHost() {
            return NetworkManager.Singleton.IsHost;
        }
    }
}
