using HarmonyLib;
using ThatCore.Valheim.Lifecycle.Patches;
using ThatCore.Valheim.Network;

namespace ThatCore.Valheim;

public static class PatchManager
{
    public static void ApplyPatches(this Harmony harmony)
    {
        // Lifecycle
        FejdStartup_TriggerLifecycle_Patch.Patch(harmony);
        Game_FindSpawnPoint_TriggerLifecycle_Patch.Patch(harmony);
        ZNet_OnNewConnection_TriggerSync_Patch.Patch(harmony);
        ZNet_Update_Patch.Patch(harmony);

        // Network
        OutgoingMessageService.Cleanup.Patch(harmony);
    }
}
