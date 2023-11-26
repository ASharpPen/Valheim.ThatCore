using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using HarmonyLib;
using ThatCore.Lifecycle;
using ThatCore.Logging;

namespace ThatCore.Network;

public static class OutgoingMessageService
{
    /// <summary>
    /// Must be lower than ca. 8000 for things like ZDO's to have a chance at being synced.
    /// </summary>
    private const int MaxQueueSizeForDispatch = 5000;

    private static Dictionary<string, Queue<OutgoingMessage>> SocketQueues { get; } = new();

    private static BinaryFormatter BinaryFormatter { get; } = new();

    static OutgoingMessageService()
    {
        LifecycleManager.OnZnetUpdate += Dispatch;

        LifecycleManager.SubscribeToWorldInit(() =>
        {
            SocketQueues.Clear();
        });
    }

    public static void AddToQueue(IMessage dto, string rpcRoute, ZRpc zrpc)
    {
        AddToQueueAsync(dto, rpcRoute, zrpc);
    }

    private static Task AddToQueueAsync(IMessage message, string rpcRoute, ZRpc zrpc)
    {
        message.Initialize();

        // Prepare messages
        var splits = SplitMessageManager.Pack(message);

        List<OutgoingMessage> outgoingMessages = new();

        foreach (var split in splits)
        {
            using MemoryStream stream = new MemoryStream();

            BinaryFormatter.Serialize(stream, split);

            ZPackage package = new();
            package.Write(stream.ToArray());

            outgoingMessages.Add(new()
            {
                Package = package,
                ZRpc = zrpc,
                Target = rpcRoute,
            });
        }

        // Add messages to queue
        string queueId = zrpc.GetSocket().GetEndPointString();

        lock (SocketQueues)
        {
            Queue<OutgoingMessage> queue;

            if (!SocketQueues.TryGetValue(queueId, out queue))
            {
                SocketQueues[queueId] = queue = new();
            }

            foreach (var outgoingMessage in outgoingMessages)
            {
                queue.Enqueue(outgoingMessage);
            }
        }

        return Task.CompletedTask;
    }

    [HarmonyPatch(typeof(ZNetPeer))]
    internal static class Cleanup
    {
        [HarmonyPatch(nameof(ZNetPeer.Dispose))]
        [HarmonyPrefix]
        private static void Dispose(ZNetPeer __instance)
        {
            var socketQueueIdentifier = __instance.m_socket.GetEndPointString();

            lock (SocketQueues)
            {
                SocketQueues.Remove(socketQueueIdentifier);
            }
        }
    }

    private static void Dispatch()
    {
        var peers = ZNet.instance.GetPeers();

        int sockets = peers.Count;

        for (int i = 0; i < sockets; ++i)
        {
            OutgoingMessageService.DispatchSocket(peers[i].m_socket);
        }
    }

    private static void DispatchSocket(ISocket socket)
    {
        if (socket is null)
        {
            return;
        }

        string queueId = socket.GetEndPointString();

        Queue<OutgoingMessage> queue;

        lock (SocketQueues)
        {
            if (!SocketQueues.TryGetValue(queueId, out queue) ||
                queue.Count == 0)
            {
                // No queue created yet. Nothing to dispatch.
                return;
            }
        }

        // Check if socket is ready for new packages.
        int queueSize = socket.GetSendQueueSize();
        if (queueSize > MaxQueueSizeForDispatch)
        {
            Log.Development?.Log("Package queue size: " + queueSize);
            return;
        }

        // Send message
        OutgoingMessage message;

        lock (SocketQueues)
        {
            message = queue.Dequeue();
        }

        try
        {
            message.ZRpc.Invoke(message.Target, message.Package);

            Log.Trace?.Log($"Sending package with size '{message.Package.Size()}' to '{message.Target}'");
        }
        catch (Exception e)
        {
            if (message.Retries > 3)
            {
                Log.Warning?.Log($"Error while trying to send package. Too many retries, will stop trying.", e);
            }
            else
            {
                lock (SocketQueues)
                {
                    // Requeue package
                    message.Retries++;
                    queue.Enqueue(message);
                }
            }
        }
    }
}
