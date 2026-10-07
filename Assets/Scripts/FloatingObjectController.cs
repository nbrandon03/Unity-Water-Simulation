using UnityEngine;

/// <summary>
/// Keeps an object attached to the water surface.
/// </summary>
public class FloatingObjectController : MonoBehaviour
{
    [Header("Water Reference")]
    public WaterController water;

    [Header("Sampling")]
    public Vector3 localSampleOffset = Vector3.zero;
    public float verticalOffset = 0.15f;

    [Header("Smoothing")]
    public float positionLerp = 6f;
    public float tiltLerp = 4f;

    [Header("Orientation")]
    public bool useSurfaceNormalForTilt = true;

    private void LateUpdate()
    {
        if (water == null)
        {
            return;
        }

        Vector3 sampleWorldPosition = transform.position + localSampleOffset;
        Vector2 worldXZ = new Vector2(sampleWorldPosition.x, sampleWorldPosition.z);
        float timeSeconds = Time.time;

        float targetHeight = water.SampleHeight(worldXZ, timeSeconds) + verticalOffset;
        Vector3 targetPosition = transform.position;
        targetPosition.y = targetHeight;

        float positionT = 1f - Mathf.Exp(-positionLerp * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, targetPosition, positionT);

        if (!useSurfaceNormalForTilt)
        {
            return;
        }

        Vector3 surfaceNormal = water.SampleNormal(worldXZ, timeSeconds);
        Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, surfaceNormal);
        float tiltT = 1f - Mathf.Exp(-tiltLerp * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, tiltT);
    }
}