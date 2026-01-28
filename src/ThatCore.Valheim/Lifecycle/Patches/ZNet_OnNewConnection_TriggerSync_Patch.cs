using HarmonyLib;

namespace ThatCore.Lifecycle.Patches;

[HarmonyPatch(typeof(ZNet))]
public static class ZNet_OnNewConnection_TriggerSync_Patch
{
    [HarmonyPatch(nameof(ZNet.OnNewConnection))]
    [HarmonyPostfix]
    private static void TriggerLifecycle(ZNetPeer peer)
    {
        LifecycleManager.PeerConnected(peer);
    }
}
