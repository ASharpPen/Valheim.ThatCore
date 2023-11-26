using System;
using System.Collections.Generic;
using ThatCore.Extensions;
using ThatCore.Logging;

namespace ThatCore.Lifecycle;

public static class LifecycleManager
{
    private static HashSet<Action> OnWorldInitActions = new HashSet<Action>();

    /// <summary>
    /// Runs when a world is entered/started.
    /// </summary>
    public static event Action OnWorldInit;

    /// <summary>
    /// Runs after all other init actions.
    /// </summary>
    public static event Action OnLateInit;

    /// <summary>
    /// Runs after OnWorldInit, when a singleplayer game is entered.
    /// </summary>
    public static event Action OnSinglePlayerInit;

    /// <summary>
    /// Runs after OnWorldInit, when a multiplayer game is joined.
    /// </summary>
    public static event Action OnMultiplayerInit;

    /// <summary>
    /// Runs after OnWorldInit, when a dedicated server is started.
    /// </summary>
    public static event Action OnDedicatedServerInit;

    public static event Action OnFindSpawnPointFirstTime;

    public static event Action<ZNetPeer> OnNewConnection;

    public static event Action OnZnetUpdate;

    public static GameState GameState { get; private set; }

    public static void SubscribeToWorldInit(Action onInit)
    {
        OnWorldInitActions.Add(onInit);
    }

    private static void WorldInit()
    {
        Log.Debug?.Log("Running world init actions");
        OnWorldInit.Raise("Error during world init event");

        foreach (var onInit in OnWorldInitActions)
        {
            onInit();
        }
    }

    internal static void InitSingleplayer()
    {
        GameState = GameState.Singleplayer;
        WorldInit();

        Log.Debug?.Log("Running singleplayer init actions");
        OnSinglePlayerInit.Raise("Error during singleplayer init event");

        OnLateInit.Raise("Error during singleplayer late init event");
    }

    internal static void InitMultiplayer()
    {
        GameState = GameState.Multiplayer;
        WorldInit();

        Log.Debug?.Log("Running multiplayer init actions");
        OnMultiplayerInit.Raise("Error during multiplayer init event");

        OnLateInit.Raise("Error during multiplayer late init event");
    }

    internal static void InitDedicated()
    {
        GameState = GameState.DedicatedServer;
        WorldInit();

        Log.Debug?.Log("Running dedicated server init actions");
        OnDedicatedServerInit.Raise("Error during dedicated server init event");

        OnLateInit.Raise("Error during dedicated server late init event");
    }

    internal static void InitFindSpawnPointFirstTime()
    {
        OnFindSpawnPointFirstTime.Raise("Error during OnFindSpawnPointFirstTime event");
    }

    internal static void PeerConnected(ZNetPeer peer)
    {
        Log.Trace?.Log($"Running OnNewConnection actions for new peer '{peer.m_playerName}:{peer.m_uid}'");
        OnNewConnection.Raise(peer);
    }

    internal static void ZnetUpdate()
    {
        OnZnetUpdate.Raise();
    }
}
