using MovementCompanyEnhanced.Core;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.Netcode;

namespace MovementCompanyEnhanced.Data {
    [Serializable]
    public class SyncedInstance<T> {
        internal static CustomMessagingManager MessageManager => NetworkManager.Singleton.CustomMessagingManager;
        internal static bool IsClient => NetworkManager.Singleton.IsClient;
        internal static bool IsHost => NetworkManager.Singleton.IsHost;

        internal static void Log (string str) => Plugin.Logger.LogInfo(str);
        internal static void LogErr (string str) => Plugin.Logger.LogError(str);

        [NonSerialized]
        protected static int IntSize = 4;

        public static T Default { get; private set; }
        public static T Instance { get; private set; }

        public static bool Synced { get; internal set; }

        protected void InitInstance(T instance) {
            Default = instance;
            Instance = instance;

            IntSize = sizeof(int);
        }

        internal static void SyncInstance(byte[] data) {
            Instance = DeserializeFromBytes(data);
            Synced = true;

            if (!Config.Instance.SYNC_TO_CLIENTS) {
                RevertSync();
                return;
            }

            Log("Successfully synced config with host.");
        }

        internal static void RevertSync() {
            Instance = Default;
            Synced = false;

            Log($"Config sync disabled. Reverted to client config.");
        }

        public static byte[] SerializeToBytes(T val) {
            BinaryFormatter bf = new();
            using MemoryStream stream = new();

            try {
                bf.Serialize(stream, val);
                return stream.ToArray();
            }
            catch (Exception e) {
                LogErr($"Error serializing instance: {e}");
                return null;
            }
        }

        public static T DeserializeFromBytes(byte[] data) {
            BinaryFormatter bf = new();
            using MemoryStream stream = new(data);

            try {
                return (T) bf.Deserialize(stream);
            } catch (Exception e) {
                LogErr($"Error deserializing instance: {e}");
                return default;
            }
        }
    }
}
