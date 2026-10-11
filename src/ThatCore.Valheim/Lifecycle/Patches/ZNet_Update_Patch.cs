using System;
using HarmonyLib;
using ThatCore.Logging;

namespace ThatCore.Valheim.Lifecycle.Patches;

public static class ZNet_Update_Patch
{
    public static void Patch(Harmony harmony)
    {
        harmony.Patch(
            original: AccessTools.Method(typeof(ZNet), nameof(ZNet.Update)),
            postfix: new HarmonyMethod(((Action)HookZnetUpdate).Method));
    }

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
