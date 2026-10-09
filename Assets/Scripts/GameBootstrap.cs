using UnityEngine;

/// <summary>Builds the playable scene when the project enters Play mode.</summary>
public static class GameBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void StartGame()
    {
        if (Object.FindFirstObjectByType<GameController>() != null)
            return;

        var game = new GameObject("Quaternion Flight Game");
        game.AddComponent<GameController>();
    }
}
