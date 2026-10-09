using UnityEngine;

/// <summary>Turns toward the player with LookRotation and Slerp; hits use distance checks.</summary>
public sealed class HomingMissile : MonoBehaviour
{
    [SerializeField] private float speed = 17.5f;
    [SerializeField] private float maximumTurnDegreesPerSecond = 70f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float hitDistance = 1.9f;
    [SerializeField] private float targetLeadTime = 0.25f;

    private Transform target;
    private float age;

    public void Initialize(Transform player, float missileSpeed, float turnRate, float maxLifetime)
    {
        target = player;
        speed = missileSpeed;
        maximumTurnDegreesPerSecond = turnRate;
        lifetime = maxLifetime;
    }

    private void Update()
    {
        if (GameController.Instance == null || GameController.Instance.IsRestarting)
            return;

        age += Time.deltaTime;
        if (age >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        FlightPlayer flightTarget = target.GetComponent<FlightPlayer>();
        Vector3 predictedPosition = target.position;
        if (flightTarget != null)
            predictedPosition += flightTarget.Velocity * targetLeadTime;

        Vector3 actualSeparation = target.position - transform.position;
        if (actualSeparation.sqrMagnitude <= hitDistance * hitDistance)
        {
            GameController.Instance.RegisterHit(this);
            return;
        }

        Vector3 direction = predictedPosition - transform.position;
        Quaternion targetRotation = SafeLookRotation(direction);
        float angleToTarget = Quaternion.Angle(transform.rotation, targetRotation);
        float maximumTurnThisFrame = maximumTurnDegreesPerSecond * Time.deltaTime;
        float slerpAmount = angleToTarget > 0f
            ? Mathf.Min(1f, maximumTurnThisFrame / angleToTarget)
            : 1f;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, slerpAmount);
        transform.position += transform.forward * (speed * Time.deltaTime);
    }

    public static Quaternion SafeLookRotation(Vector3 direction)
    {
        Vector3 forward = direction.normalized;
        Vector3 up = Mathf.Abs(Vector3.Dot(forward, Vector3.up)) > 0.98f
            ? Vector3.forward
            : Vector3.up;
        return Quaternion.LookRotation(forward, up);
    }
}
