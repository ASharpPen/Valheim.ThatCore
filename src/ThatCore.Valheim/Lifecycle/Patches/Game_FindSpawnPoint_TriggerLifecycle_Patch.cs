using System;
using HarmonyLib;

namespace ThatCore.Valheim.Lifecycle.Patches;

public static class Game_FindSpawnPoint_TriggerLifecycle_Patch
{
    private static bool FirstTime = true;

    static Game_FindSpawnPoint_TriggerLifecycle_Patch()
    {
        LifecycleManager.SubscribeToWorldInit(() =>
        {
            FirstTime = true;
        });
    }

    public static void Patch(Harmony harmony)
    {
        harmony.Patch(
            original: AccessTools.Method(typeof(Game), nameof(Game.FindSpawnPoint)),
            postfix: new HarmonyMethod(((Action)TriggerLifecycle).Method));
    }

    private static void TriggerLifecycle()
    {
        if (FirstTime)
        {
            FirstTime = false;
            LifecycleManager.InitFindSpawnPointFirstTime();
        }
    }
}
