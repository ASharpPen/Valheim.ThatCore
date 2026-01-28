using System;
using HarmonyLib;
using ThatCore.Logging;

namespace ThatCore.Lifecycle.Patches;

[HarmonyPatch]
public static class ZNet_Update_Patch
{
    [HarmonyPatch(typeof(ZNet), nameof(ZNet.Update))]
    [HarmonyPostfix]
    private static void HookZnetUpdate()
    {
        try
        {
            LifecycleManager.ZnetUpdate();
        }
        catch (Exception e)
        {
            Log.Error?.Log($"Error during {nameof(LifecycleManager.ZnetUpdate)} for {nameof(ZNet)}.{nameof(ZNet.Update)}", e);
        }
    }
}
