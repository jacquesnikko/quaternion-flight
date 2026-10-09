using System.Collections;
using UnityEngine;

/// <summary>Owns the generated scene, camera, score, hit count, and automatic restart.</summary>
public sealed class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }

    public float SurvivalSeconds { get; private set; }
    public bool IsRestarting { get; private set; }

    private const int HitsToRestart = 5;
    private FlightPlayer player;
    private MissileSpawner spawner;
    private Camera gameCamera;
    private int hits;
    private GUIStyle hudStyle;
    private GUIStyle titleStyle;
    private static Mesh unitBlockMesh;
    private int activeMissileCount;
    private float nextMissileCountRefresh;

    private void Awake()
    {
        Instance = this;
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 60;
        RenderSettings.ambientLight = new Color(0.35f, 0.4f, 0.55f);
        BuildWorld();
    }

    private void Update()
    {
        if (!IsRestarting)
            SurvivalSeconds += Time.deltaTime;

        if (Time.unscaledTime >= nextMissileCountRefresh)
        {
            activeMissileCount = Object.FindObjectsByType<HomingMissile>(FindObjectsSortMode.None).Length;
            nextMissileCountRefresh = Time.unscaledTime + 0.25f;
        }
    }

    private void LateUpdate()
    {
        if (player == null || gameCamera == null)
            return;

        Vector3 desiredPosition = player.transform.position - player.transform.forward * 8f +
                                  Vector3.up * 3.2f;
        gameCamera.transform.position = Vector3.Lerp(
            gameCamera.transform.position, desiredPosition, 5f * Time.deltaTime);
        Vector3 lookPoint = player.transform.position + player.transform.forward * 8f;
        Quaternion lookRotation = Quaternion.LookRotation(lookPoint - gameCamera.transform.position);
        gameCamera.transform.rotation = Quaternion.Slerp(
            gameCamera.transform.rotation, lookRotation, 5f * Time.deltaTime);
    }

    private void BuildWorld()
    {
        gameCamera = new GameObject("Follow Camera").AddComponent<Camera>();
        gameCamera.fieldOfView = 62f;
        gameCamera.nearClipPlane = 0.1f;
        gameCamera.farClipPlane = 400f;
        gameCamera.transform.position = new Vector3(0f, 3.2f, -8f);
        gameCamera.transform.rotation = Quaternion.LookRotation(new Vector3(0f, -0.1f, 1f));
        gameCamera.backgroundColor = new Color(0.015f, 0.025f, 0.07f);
        gameCamera.clearFlags = CameraClearFlags.SolidColor;

        var keyLight = new GameObject("Aircraft Key Light").AddComponent<Light>();
        keyLight.type = LightType.Directional;
        keyLight.intensity = 1.35f;
        keyLight.transform.rotation = Quaternion.Euler(35f, -30f, 0f);

        var playerObject = new GameObject("Player Aircraft");
        player = playerObject.AddComponent<FlightPlayer>();
        player.BuildVisual();

        var spawnerObject = new GameObject("Missile Spawner");
        spawner = spawnerObject.AddComponent<MissileSpawner>();
        spawner.Initialize(player, gameCamera);
    }

    public void RegisterHit(HomingMissile missile)
    {
        if (IsRestarting)
            return;

        if (missile != null)
            Destroy(missile.gameObject);

        hits++;
        if (hits >= HitsToRestart)
        {
            IsRestarting = true;
            StartCoroutine(RestartAfterDelay());
        }
    }

    private IEnumerator RestartAfterDelay()
    {
        yield return new WaitForSecondsRealtime(1.5f);
        foreach (HomingMissile missile in Object.FindObjectsByType<HomingMissile>(FindObjectsSortMode.None))
            Destroy(missile.gameObject);

        hits = 0;
        SurvivalSeconds = 0f;
        IsRestarting = false;
        player.ResetFlight();
        spawner.Initialize(player, gameCamera);
    }

    private void OnGUI()
    {
        if (hudStyle == null)
        {
            hudStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Max(16, Screen.height / 38),
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            titleStyle = new GUIStyle(hudStyle)
            {
                fontSize = Mathf.Max(30, Screen.height / 18),
                alignment = TextAnchor.MiddleCenter
            };
        }

        GUI.Label(new Rect(22, 18, 340, 38), $"SURVIVED  {SurvivalSeconds:0.0}s", hudStyle);
        GUI.Label(new Rect(22, 55, 340, 38), $"HITS  {hits} / {HitsToRestart}", hudStyle);
        GUI.Label(new Rect(22, 92, 420, 38), $"INCOMING  {activeMissileCount}", hudStyle);
        GUI.Label(new Rect(22, 129, 700, 38), "DODGE  HOLD  A / D  or  ← / →  TO CURVE", hudStyle);

        if (IsRestarting)
        {
            GUI.color = new Color(1f, 0.48f, 0.24f);
            GUI.Label(new Rect(0, Screen.height * 0.36f, Screen.width, 80),
                "AIRCRAFT LOST — RESTARTING", titleStyle);
            GUI.color = Color.white;
        }
    }

    public static GameObject MakeBlock(string objectName, Transform parent, Vector3 localPosition,
        Vector3 localScale, Color color)
    {
        GameObject block = new GameObject(objectName);
        block.name = objectName;
        block.transform.SetParent(parent, false);
        block.transform.localPosition = localPosition;
        block.transform.localScale = localScale;

        var filter = block.AddComponent<MeshFilter>();
        if (unitBlockMesh == null)
            unitBlockMesh = CreateUnitBlockMesh();
        filter.sharedMesh = unitBlockMesh;

        Shader shader = Shader.Find("Standard");
        var material = new Material(shader) { color = color };
        block.AddComponent<MeshRenderer>().sharedMaterial = material;
        return block;
    }

    private static Mesh CreateUnitBlockMesh()
    {
        var vertices = new System.Collections.Generic.List<Vector3>(24);
        var triangles = new System.Collections.Generic.List<int>(36);
        AddFace(vertices, triangles, new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(0.5f, -0.5f, 0.5f),
            new Vector3(0.5f, 0.5f, 0.5f), new Vector3(-0.5f, 0.5f, 0.5f));
        AddFace(vertices, triangles, new Vector3(0.5f, -0.5f, -0.5f), new Vector3(-0.5f, -0.5f, -0.5f),
            new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(0.5f, 0.5f, -0.5f));
        AddFace(vertices, triangles, new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(-0.5f, -0.5f, 0.5f),
            new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(-0.5f, 0.5f, -0.5f));
        AddFace(vertices, triangles, new Vector3(0.5f, -0.5f, 0.5f), new Vector3(0.5f, -0.5f, -0.5f),
            new Vector3(0.5f, 0.5f, -0.5f), new Vector3(0.5f, 0.5f, 0.5f));
        AddFace(vertices, triangles, new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(0.5f, 0.5f, 0.5f),
            new Vector3(0.5f, 0.5f, -0.5f), new Vector3(-0.5f, 0.5f, -0.5f));
        AddFace(vertices, triangles, new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.5f, -0.5f, -0.5f),
            new Vector3(0.5f, -0.5f, 0.5f), new Vector3(-0.5f, -0.5f, 0.5f));

        var mesh = new Mesh { name = "Unit Block Visual Mesh" };
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        return mesh;
    }

    private static void AddFace(System.Collections.Generic.List<Vector3> vertices,
        System.Collections.Generic.List<int> triangles, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        int first = vertices.Count;
        vertices.Add(a);
        vertices.Add(b);
        vertices.Add(c);
        vertices.Add(d);
        triangles.Add(first);
        triangles.Add(first + 1);
        triangles.Add(first + 2);
        triangles.Add(first);
        triangles.Add(first + 2);
        triangles.Add(first + 3);
    }
}
