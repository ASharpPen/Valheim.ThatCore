using System;
using System.Collections.Generic;
using System.Linq;
using ThatCore.Logging;

namespace ThatCore.Network;

internal class IncomingMessage
{
    public IncomingMessage(SplitMessage initialMessage)
    {
        TransferId = initialMessage.TransferId;
        ExpectedSplits = initialMessage.SplitCount;
        MissingSplits = Enumerable
            .Range(0, ExpectedSplits)
            .ToHashSet();

        Splits = new()
            {
                { initialMessage.SplitIndex, initialMessage }
            };
        MissingSplits.Remove(initialMessage.SplitIndex);
    }

    public Guid TransferId { get; }

    public int ExpectedSplits { get; }

    public DateTimeOffset SessionStarted { get; } = DateTimeOffset.UtcNow;

    public DateTimeOffset LastSplit { get; set; } = DateTimeOffset.UtcNow;

    public Dictionary<int, SplitMessage> Splits { get; }

    public HashSet<int> MissingSplits { get; }

    public void AddPackage(SplitMessage split)
    {
        if (!ValidPackage(split))
        {
            Log.Debug?.Log($"Received unexpected misformed package '{split.TransferId}:{split.SplitIndex}' for session '{split.TransferId}'. Dropping package.");
            return;
        }

        Splits[split.SplitIndex] = split;
        MissingSplits.Remove(split.SplitIndex);

        LastSplit = DateTimeOffset.UtcNow;
    }

    private bool ValidPackage(SplitMessage split) =>
        TransferId == split.TransferId &&
        ExpectedSplits == split.SplitCount
        ;
}