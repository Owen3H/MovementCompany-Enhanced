using MovementCompanyEnhanced.Core;
using System;
using System.IO;
using System.Runtime.Serialization;
using Unity.Netcode;

namespace MovementCompanyEnhanced.Data;

[Serializable]
public class SyncedInstance<T> {
    public static CustomMessagingManager MessageManager => NetworkManager.Singleton.CustomMessagingManager;
    public static bool IsClient => NetworkManager.Singleton.IsClient;
    public static bool IsHost => NetworkManager.Singleton.IsHost;

    internal static void Log (string str) => Plugin.Logger.LogInfo(str);
    internal static void LogDebug (string str) => Plugin.Logger.LogDebug(str);
    internal static void LogErr (string str) => Plugin.Logger.LogError(str);

    [NonSerialized] 
    protected static int INT_SIZE = 4;

    [NonSerialized] 
    static readonly DataContractSerializer serializer = new(typeof(T));

    internal static T Default { get; private set; }
    internal static T Instance { get; private set; }

    internal static bool Synced;

    protected void InitInstance(T instance) {
        Default = instance;
        Instance = instance;

        INT_SIZE = sizeof(int);
    }

    internal static void SyncInstance(byte[] data) {
        Instance = DeserializeFromBytes(data);
        Synced = true;

        if (!Config.Instance.SYNC_TO_CLIENTS) {
            RevertSync(log: false);
            return;
        }

        Log("Successfully synced config with host.");
    }

    internal static void RevertSync(bool log = true) {
        Instance = Default;
        Synced = false;

        if (!log) return;
        Log($"Config sync disabled. Reverted to client config.");
    }

    public static byte[] SerializeToBytes(T val) {
        using MemoryStream stream = new();

        try {
            serializer.WriteObject(stream, val);
            return stream.ToArray();
        }
        catch (Exception e) {
            LogErr($"Error serializing instance: {e}");
            return null;
        }
    }

    public static T DeserializeFromBytes(byte[] data) {
        using MemoryStream stream = new(data);

        try {
            return (T) serializer.ReadObject(stream);
        } catch (Exception e) {
            LogErr($"Error deserializing instance: {e}");
            return default;
        }
    }

    internal static void SendMessage(string label, ulong clientId, FastBufferWriter stream) {
        bool fragment = stream.Capacity > stream.MaxCapacity;
        NetworkDelivery delivery = fragment ? NetworkDelivery.ReliableFragmentedSequenced : NetworkDelivery.Reliable;

        if (fragment) {
            LogDebug(
                $"Size of stream ({stream.Capacity}) was past the max buffer size.\n" +
                "Config instance will be sent in fragments to avoid overflowing the buffer."
            );
        }

        MessageManager.SendNamedMessage(label, clientId, stream, delivery);
    }
}