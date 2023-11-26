using System;

namespace ThatCore.Network;

// TODO: This whole thing is most likely better dealt with by having known parameters added to the ZPackage directly.
// That should remove the need for serializing the SplitPackage object itself.
[Serializable]
public class SplitMessage
{
    public int SplitIndex { get; set; }

    public int SplitCount { get; set; }

    /// <summary>
    /// Id shared by all all split of same package.
    /// </summary>
    public Guid TransferId { get; set; }

    public byte[] Content { get; set; }
}
