using HarmonyLib;
using ThatCore.Lifecycle.Patches;

namespace ThatCore.Lifecycle;

public static class PatchManager
{
    public static void ApplyPatches(this Harmony harmony)
    {
        harmony.PatchAll(typeof(FejdStartup_TriggerLifecycle_Patch));
        harmony.PatchAll(typeof(Game_FindSpawnPoint_TriggerLifecycle_Patch));
        harmony.PatchAll(typeof(ZNet_OnNewConnection_TriggerSync_Patch));
        harmony.PatchAll(typeof(ZNet_Update_Patch));
    }
}
