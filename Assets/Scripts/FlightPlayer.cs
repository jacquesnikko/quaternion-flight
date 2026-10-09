using UnityEngine;

/// <summary>Constant forward flight with small, quaternion-based steering.</summary>
public sealed class FlightPlayer : MonoBehaviour
{
    [SerializeField] private float forwardSpeed = 12f;
    [SerializeField] private float yawDegreesPerSecond = 65f;
    [SerializeField] private float bankDegrees = 18f;
    [SerializeField] private float bankSmoothing = 7f;

    private Transform visualRoot;
    private float horizontalInput;

    public Vector3 Velocity => transform.forward * forwardSpeed;

    public void BuildVisual()
    {
        visualRoot = new GameObject("Aircraft Visual").transform;
        visualRoot.SetParent(transform, false);

        GameController.MakeBlock("Fuselage", visualRoot, Vector3.zero,
            new Vector3(0.65f, 0.45f, 2.2f), new Color(0.1f, 0.78f, 0.92f));
        GameController.MakeBlock("Wings", visualRoot, new Vector3(0f, 0f, -0.1f),
            new Vector3(3.2f, 0.12f, 0.72f), new Color(0.08f, 0.42f, 0.78f));
        GameController.MakeBlock("Tail", visualRoot, new Vector3(0f, 0.32f, -0.78f),
            new Vector3(1.05f, 0.1f, 0.48f), new Color(0.08f, 0.42f, 0.78f));
        GameController.MakeBlock("Canopy", visualRoot, new Vector3(0f, 0.3f, 0.38f),
            new Vector3(0.38f, 0.22f, 0.62f), new Color(0.75f, 0.95f, 1f));
    }

    private void Update()
    {
        if (GameController.Instance == null || GameController.Instance.IsRestarting)
            return;

        horizontalInput = 0f;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) horizontalInput -= 1f;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) horizontalInput += 1f;

        // Apply a small heading change using quaternion multiplication.
        Quaternion yawStep = Quaternion.AngleAxis(
            horizontalInput * yawDegreesPerSecond * Time.deltaTime, Vector3.up);
        transform.rotation = yawStep * transform.rotation;

        // Bank the aircraft visually while it turns.
        Quaternion bankTarget = Quaternion.Euler(0f, 0f, -horizontalInput * bankDegrees);
        visualRoot.localRotation = Quaternion.Slerp(
            visualRoot.localRotation, bankTarget, bankSmoothing * Time.deltaTime);

        transform.position += transform.forward * (forwardSpeed * Time.deltaTime);
    }

    public void ResetFlight()
    {
        transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        visualRoot.localRotation = Quaternion.identity;
    }
}
