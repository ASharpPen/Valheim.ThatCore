using System;
using HarmonyLib;

namespace ThatCore.Valheim.Lifecycle.Patches;

public static class ZNet_OnNewConnection_TriggerSync_Patch
{
    public static void Patch(Harmony harmony)
    {
        harmony.Patch(
            original: AccessTools.Method(typeof(ZNet), nameof(ZNet.OnNewConnection)),
            postfix: new HarmonyMethod(((Action<ZNetPeer>)LifecycleManager.PeerConnected).Method));
    }
}
