using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ThatCore.Logging;

namespace ThatCore.Network;

public static class SplitMessageManager
{
    internal static int MaxPackageSize = 100_000;

    public static List<SplitMessage> Pack(IMessage message)
    {
        var messageContent = SerializerManager
            .GetSerializer(message.GetType())
            .SerializeAndCompress(message);

        int splitCount = Math.Max(1, (int)Math.Ceiling(messageContent.Count() / (double)MaxPackageSize));

        Guid transferId = Guid.NewGuid();

        List<SplitMessage> splits = new(splitCount);

        for (int i = 0; i < splitCount; ++i)
        {
            int remaining = messageContent.Count() - (i * MaxPackageSize);
            int splitItems = Math.Min(remaining, MaxPackageSize);
            int start = i * MaxPackageSize;

            var split = new byte[splitItems];
            Array.Copy(messageContent, start, split, 0, splitItems);

            splits.Add(new SplitMessage
            {
                SplitIndex = i,
                SplitCount = splitCount,
                TransferId = transferId,
                Content = split,
            });

            Log.Development?.Log($"Created package:" +
                $"\n\t\ti: {i}" +
                $"\n\t\tRemaining: {remaining}" +
                $"\n\t\tItems: {splitItems}" +
                $"\n\t\tStart: {start}" +
                $"\n\t\tSplit Length: {split.Length}" +
                $"\n\t\tTransferId: {splits.Last().TransferId}"
                );
        }

        Log.Development?.Log($"Split package into {splits.Count} pieces");

        return splits;
    }

    public static IMessage CombineAndUnpackSplits(Type type, Dictionary<int, SplitMessage> splits)
    {
        using MemoryStream serializedStream = new(splits
            .OrderBy(x => x.Key)
            .SelectMany(x => x.Value.Content)
            .ToArray());

        Log.Development?.Log($"Deserializing '{splits.Count}' split packages with total size of '{serializedStream.Length}' bytes");

        Log.Debug?.Log($"Deserializing package size: {serializedStream.Length} bytes");

        return SerializerManager.GetSerializer(type)
            .DeserializeCompressed(type, serializedStream) as IMessage;
    }

    public static T CombineAndUnpackSplits<T>(Dictionary<int, SplitMessage> splits)
    {
        using MemoryStream serializedStream = new(splits
            .OrderBy(x => x.Key)
            .SelectMany(x => x.Value.Content)
            .ToArray());

        Log.Development?.Log($"Deserializing '{splits.Count}' split packages with total size of '{serializedStream.Length}' bytes");

        Log.Debug?.Log($"Deserializing package size: {serializedStream.Length} bytes");

        return SerializerManager.GetSerializer<T>()
            .DeserializeCompressed<T>(serializedStream);
    }
}
