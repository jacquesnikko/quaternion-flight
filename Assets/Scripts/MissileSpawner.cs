using UnityEngine;

/// <summary>Spawns each wave beyond the camera frustum; wave size scales with survival time.</summary>
public sealed class MissileSpawner : MonoBehaviour
{
    [SerializeField] private float secondsBetweenWaves = 6.5f;
    [SerializeField] private int startingMissiles = 1;
    [SerializeField] private int addedMissilesEveryTenSeconds = 1;
    [SerializeField] private int maximumMissilesPerWave = 3;
    [SerializeField] private int maximumActiveMissiles = 3;
    [SerializeField] private float spawnDepth = 14f;

    private float countdown;
    private FlightPlayer player;
    private Camera gameCamera;

    public void Initialize(FlightPlayer flightPlayer, Camera followCamera)
    {
        player = flightPlayer;
        gameCamera = followCamera;
        countdown = 2.5f;
    }

    private void Update()
    {
        if (player == null || gameCamera == null || GameController.Instance == null ||
            GameController.Instance.IsRestarting)
            return;

        countdown -= Time.deltaTime;
        if (countdown > 0f)
            return;

        countdown = secondsBetweenWaves;
        int difficultySteps = Mathf.FloorToInt(GameController.Instance.SurvivalSeconds / 10f);
        int missileCount = Mathf.Clamp(
            startingMissiles + difficultySteps * addedMissilesEveryTenSeconds,
            1, maximumMissilesPerWave);

        int activeMissiles = Object.FindObjectsByType<HomingMissile>(FindObjectsSortMode.None).Length;
        int availableSlots = Mathf.Max(0, maximumActiveMissiles - activeMissiles);
        int missilesToSpawn = Mathf.Min(missileCount, availableSlots);

        for (int i = 0; i < missilesToSpawn; i++)
            SpawnOutsideCameraView(i);
    }

    private void SpawnOutsideCameraView(int index)
    {
        float halfHeight = spawnDepth * Mathf.Tan(gameCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float halfWidth = halfHeight * gameCamera.aspect;
        // Spawn just beyond a side edge while remaining ahead of the aircraft.
        float side = index % 2 == 0 ? -1f : 1f;
        if (Random.value < 0.5f)
            side *= -1f;
        float horizontalOffset = side * (halfWidth + Random.Range(2f, 4f));
        float verticalOffset = Random.Range(-halfHeight * 0.35f, halfHeight * 0.35f);
        Vector3 position = gameCamera.transform.position +
                           gameCamera.transform.forward * spawnDepth +
                           gameCamera.transform.right * horizontalOffset +
                           gameCamera.transform.up * verticalOffset;

        var missile = new GameObject("Homing Missile");
        missile.transform.position = position;
        missile.transform.rotation = HomingMissile.SafeLookRotation(player.transform.position - position);
        BuildMissileVisual(missile.transform);
        var homing = missile.AddComponent<HomingMissile>();
        homing.Initialize(player.transform, 17.5f, 70f, 5f);
    }

    private static void BuildMissileVisual(Transform parent)
    {
        GameController.MakeBlock("Missile Body", parent, Vector3.zero,
            new Vector3(0.55f, 0.55f, 1.5f), new Color(1f, 0.32f, 0.12f));
        GameController.MakeBlock("Missile Nose", parent, new Vector3(0f, 0f, 0.72f),
            new Vector3(0.36f, 0.36f, 0.4f), new Color(1f, 0.82f, 0.22f));
        GameController.MakeBlock("Missile Fins", parent, new Vector3(0f, 0f, -0.48f),
            new Vector3(1.05f, 0.12f, 0.42f), new Color(0.72f, 0.12f, 0.08f));
    }
}
