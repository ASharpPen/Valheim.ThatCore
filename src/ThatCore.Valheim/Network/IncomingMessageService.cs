using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using ThatCore.Lifecycle;
using ThatCore.Logging;

namespace ThatCore.Network;

public static class IncomingMessageService
{
    private static Dictionary<Guid, IncomingMessage> MessageCache { get; } = new();
    private static Dictionary<Guid, DateTimeOffset> RecentlyDroppedTransfers { get; } = new();

    private static TimeSpan RetentionTime { get; } = TimeSpan.FromMinutes(1);
    private static TimeSpan DroppedRetentionTime { get; } = TimeSpan.FromMinutes(2);

    private static Queue<IMessage> ReceivedMessages { get; } = new();
    private static BinaryFormatter BinaryFormatter { get; } = new();

    static IncomingMessageService()
    {
        LifecycleManager.OnZnetUpdate += UpdateCache;
        LifecycleManager.OnZnetUpdate += DispatchMessage;

        LifecycleManager.SubscribeToWorldInit(() =>
        {
            MessageCache.Clear();
            RecentlyDroppedTransfers.Clear();
            ReceivedMessages.Clear();
        });
    }

    private static void UpdateCache()
    {
        if (RecentlyDroppedTransfers.Count > 0)
        {
            DateTimeOffset droppedRetentionTimestamp = DateTimeOffset.UtcNow - DroppedRetentionTime;

            foreach (var droppedTransfer in RecentlyDroppedTransfers.ToArray())
            {
                if (droppedTransfer.Value < droppedRetentionTimestamp)
                {
                    RecentlyDroppedTransfers.Remove(droppedTransfer.Key);
                }
            }
        }

        if (MessageCache.Count > 0)
        {
            DateTimeOffset retentionTimestamp = DateTimeOffset.UtcNow - RetentionTime;

            foreach (var session in MessageCache.Values.ToArray())
            {
                if (session.LastSplit < retentionTimestamp)
                {
                    Log.Trace?.Log($"Removing dead package session with transfer id '{session.TransferId}' and {session.Splits.Count}/{session.ExpectedSplits} received packages");

                    MessageCache.Remove(session.TransferId);
                    RecentlyDroppedTransfers[session.TransferId] = DateTimeOffset.UtcNow;
                }
            }
        }
    }

    private static void DispatchMessage()
    {
        try
        {
            IMessage item;

            lock (ReceivedMessages)
            {
                if (ReceivedMessages.Count == 0)
                {
                    return;
                }

                item = ReceivedMessages.Dequeue();
            }

            Log.Development?.Log($"[IncomingMessageService:DispatchMessage] Running followup unpacking of message '{item.GetType()}'");

            if (item is not null)
            {
                item.AfterUnpack();
            }
        }
        catch (Exception e)
        {
            Log.Error?.Log("Error while attempting to process received package.", e);
        }
    }

    public static Task ReceiveMessageAsync<T>(ZPackage package)
    {
        using MemoryStream stream = new(package.ReadByteArray());

        var split = BinaryFormatter.Deserialize(stream) as SplitMessage;

         ReceiveMessage<T>(split);

        return Task.CompletedTask;
    }

    public static void ReceiveMessage<T>(SplitMessage split) => ReceiveMessage(typeof(T), split);

    public static void ReceiveMessage(Type messageType, SplitMessage splitMessage)
    {
        if (RecentlyDroppedTransfers.ContainsKey(splitMessage.TransferId))
        {
            Log.Trace?.Log($"[IncommingMessageService:ReceiveMessage] Received part '{splitMessage.SplitIndex}' of message '{splitMessage.TransferId}' which was recently given up on.");

            RecentlyDroppedTransfers[splitMessage.TransferId] = DateTimeOffset.UtcNow;
            return;
        }

        IncomingMessage session;

        if (MessageCache.TryGetValue(splitMessage.TransferId, out session))
        {
            session.AddPackage(splitMessage);
        }
        else
        {
            MessageCache[splitMessage.TransferId] = session = new(splitMessage);
        }

        Log.Development?.Log($"[IncommingMessageService:ReceiveMessage] Missing splits {session.MissingSplits.Count} for transfer '{splitMessage.TransferId}'");

        if (session.MissingSplits.Count == 0)
        {
            var message = SplitMessageManager.CombineAndUnpackSplits(messageType, session.Splits);
            MessageCache.Remove(session.TransferId);

            lock (ReceivedMessages)
            {
                ReceivedMessages.Enqueue(message);
            }
        }
    }
}
