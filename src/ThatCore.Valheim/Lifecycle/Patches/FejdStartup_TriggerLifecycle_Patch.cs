using System;
using HarmonyLib;

namespace ThatCore.Valheim.Lifecycle.Patches;

public static class FejdStartup_TriggerLifecycle_Patch
{
    public static void Patch(Harmony harmony)
    {
        var patchType = typeof(FejdStartup);

        // Singleplayer
        harmony.Patch(
            original: AccessTools.Method(patchType, nameof(FejdStartup.OnWorldStart)),
            prefix: new HarmonyMethod(((Action)LifecycleManager.InitSingleplayer).Method));

        // Multiplayer
        harmony.Patch(
            original: AccessTools.Method(patchType, nameof(FejdStartup.JoinServer)),
            prefix: new HarmonyMethod(((Action)LifecycleManager.InitMultiplayer).Method));

        // Server
        harmony.Patch(
            original: AccessTools.Method(patchType, nameof(FejdStartup.ParseServerArguments)),
            prefix: new HarmonyMethod(((Action)LifecycleManager.InitDedicated).Method));
    }
}
