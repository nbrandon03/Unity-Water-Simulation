using UnityEngine;

/// <summary>
/// Stores the wave configuration shared by the water surface and gameplay systems.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class WaterController : MonoBehaviour
{
    [System.Serializable]
    public struct Wave
    {
        [Tooltip("2D travel direction in the XZ plane.")]
        public Vector2 direction;

        [Tooltip("Vertical amplitude of the wave.")]
        public float amplitude;

        [Tooltip("Distance between crests.")]
        public float wavelength;

        [Tooltip("Temporal speed term.")]
        public float speed;

        [Range(0f, 1.5f)]
        [Tooltip("Controls horizontal crest sharpening.")]
        public float steepness;

        [Tooltip("Phase offset in radians.")]
        public float phase;
    }

    public const int MaxWaveCount = 8;

    [Header("Renderer / Material")]
    public Renderer targetRenderer;

    [Tooltip("Legacy compatibility field used by the current scene.")]
    public Material waterMaterial;

    [Header("Legacy Wave Controls")]
    public Vector2 direction1 = new Vector2(1f, 0f);
    public float amplitude1 = 0.5f;
    public float frequency1 = 0.5f;
    public float speed1 = 1.0f;
    public float steepness1 = 1.0f;

    public Vector2 direction2 = new Vector2(0f, 1f);
    public float amplitude2 = 0.3f;
    public float frequency2 = 0.8f;
    public float speed2 = 1.2f;
    public float steepness2 = 0.8f;

    public Vector2 direction3 = new Vector2(1f, 1f);
    public float amplitude3 = 0.4f;
    public float frequency3 = 0.6f;
    public float speed3 = 0.8f;
    public float steepness3 = 0.7f;

    public Vector2 direction4 = new Vector2(-1f, 1f);
    public float amplitude4 = 0.2f;
    public float frequency4 = 1.0f;
    public float speed4 = 1.5f;
    public float steepness4 = 1.1f;

    [Header("Wave Set")]
    [Tooltip("At least four waves are recommended for convincing motion.")]
    public Wave[] waves = new Wave[]
    {
        new Wave { direction = new Vector2( 1.0f,  0.1f), amplitude = 0.60f, wavelength = 10f, speed = 1.30f, steepness = 0.45f, phase = 0.0f },
        new Wave { direction = new Vector2( 0.4f,  1.0f), amplitude = 0.35f, wavelength =  6f, speed = 1.80f, steepness = 0.35f, phase = 1.2f },
        new Wave { direction = new Vector2(-0.7f,  0.4f), amplitude = 0.22f, wavelength =  4f, speed = 2.20f, steepness = 0.25f, phase = 2.1f },
        new Wave { direction = new Vector2(-1.0f, -0.2f), amplitude = 0.15f, wavelength =  2.5f, speed = 3.10f, steepness = 0.15f, phase = 0.7f }
    };

    [Header("Optional Detail Layer")]
    public Texture2D detailTexture;
    public Vector2 detailTiling = new Vector2(6f, 6f);
    public Vector2 detailScroll = new Vector2(0.05f, 0.03f);
    [Range(0f, 1f)] public float detailStrength = 0.08f;

    private Material runtimeMaterial;

    private void Reset()
    {
        targetRenderer = GetComponent<Renderer>();
        SyncWaveArrayFromLegacyFields();
    }

    private void Awake()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
        }

        SyncWaveArrayFromLegacyFields();
    }

    private void OnValidate()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
        }

        SyncWaveArrayFromLegacyFields();
    }

    private void Update()
    {
        ApplyShaderParameters(Time.time);
    }

    private void SyncWaveArrayFromLegacyFields()
    {
        waves = new[]
        {
            new Wave { direction = direction1, amplitude = amplitude1, wavelength = Mathf.Max(0.0001f, 2f * Mathf.PI / Mathf.Max(0.0001f, frequency1)), speed = speed1, steepness = steepness1, phase = 0f },
            new Wave { direction = direction2, amplitude = amplitude2, wavelength = Mathf.Max(0.0001f, 2f * Mathf.PI / Mathf.Max(0.0001f, frequency2)), speed = speed2, steepness = steepness2, phase = 0f },
            new Wave { direction = direction3, amplitude = amplitude3, wavelength = Mathf.Max(0.0001f, 2f * Mathf.PI / Mathf.Max(0.0001f, frequency3)), speed = speed3, steepness = steepness3, phase = 0f },
            new Wave { direction = direction4, amplitude = amplitude4, wavelength = Mathf.Max(0.0001f, 2f * Mathf.PI / Mathf.Max(0.0001f, frequency4)), speed = speed4, steepness = steepness4, phase = 0f }
        };
    }

    private Material GetMaterialInstance()
    {
        if (waterMaterial != null)
        {
            return waterMaterial;
        }

        if (targetRenderer == null)
        {
            return null;
        }

        if (runtimeMaterial == null)
        {
            runtimeMaterial = Application.isPlaying ? targetRenderer.material : targetRenderer.sharedMaterial;
        }

        return runtimeMaterial;
    }

    private void ApplyShaderParameters(float timeSeconds)
    {
        Material material = GetMaterialInstance();
        if (material == null)
        {
            return;
        }

        material.SetFloat("_CustomTime", timeSeconds);

        material.SetVector("_WaveDir1", new Vector4(direction1.normalized.x, direction1.normalized.y, 0f, 0f));
        material.SetFloat("_Amplitude1", amplitude1);
        material.SetFloat("_Frequency1", frequency1);
        material.SetFloat("_Speed1", speed1);
        material.SetFloat("_Steepness1", steepness1);

        material.SetVector("_WaveDir2", new Vector4(direction2.normalized.x, direction2.normalized.y, 0f, 0f));
        material.SetFloat("_Amplitude2", amplitude2);
        material.SetFloat("_Frequency2", frequency2);
        material.SetFloat("_Speed2", speed2);
        material.SetFloat("_Steepness2", steepness2);

        material.SetVector("_WaveDir3", new Vector4(direction3.normalized.x, direction3.normalized.y, 0f, 0f));
        material.SetFloat("_Amplitude3", amplitude3);
        material.SetFloat("_Frequency3", frequency3);
        material.SetFloat("_Speed3", speed3);
        material.SetFloat("_Steepness3", steepness3);

        material.SetVector("_WaveDir4", new Vector4(direction4.normalized.x, direction4.normalized.y, 0f, 0f));
        material.SetFloat("_Amplitude4", amplitude4);
        material.SetFloat("_Frequency4", frequency4);
        material.SetFloat("_Speed4", speed4);
        material.SetFloat("_Steepness4", steepness4);
    }

    /// <summary>
    /// Returns the summed displacement from all active waves at the given XZ location and time.
    /// </summary>
    public Vector3 SampleDisplacement(Vector2 worldXZ, float timeSeconds)
    {
        Vector3 displacement = Vector3.zero;

        if (waves != null)
        {
            int waveCount = Mathf.Min(waves.Length, MaxWaveCount);
            for (int i = 0; i < waveCount; i++)
            {
                Wave wave = waves[i];
                Vector2 direction = wave.direction.sqrMagnitude > 0.0001f ? wave.direction.normalized : Vector2.right;
                float wavelength = Mathf.Max(0.0001f, wave.wavelength);
                float k = 2f * Mathf.PI / wavelength;
                float phase = k * Vector2.Dot(direction, worldXZ) + wave.speed * timeSeconds + wave.phase;

                float waveCos = Mathf.Cos(phase);
                float waveSin = Mathf.Sin(phase);
                float horizontal = wave.steepness * wave.amplitude * waveCos;

                displacement.x += direction.x * horizontal;
                displacement.z += direction.y * horizontal;
                displacement.y += wave.amplitude * waveSin;
            }
        }

        displacement.y += SampleDetailHeight(worldXZ, timeSeconds);
        return displacement;
    }

    private float SampleDetailHeight(Vector2 worldXZ, float timeSeconds)
    {
        if (detailTexture == null)
        {
            return 0f;
        }

        Vector2 uv = Vector2.Scale(worldXZ, detailTiling) + detailScroll * timeSeconds;
        float u = Mathf.Repeat(uv.x, 1f);
        float v = Mathf.Repeat(uv.y, 1f);

        float detailSample = detailTexture.GetPixelBilinear(u, v).grayscale;
        return (detailSample - 0.5f) * 2f * detailStrength;
    }

    /// <summary>
    /// Returns the world-space height of the water at the given XZ location and time.
    /// </summary>
    public float SampleHeight(Vector2 worldXZ, float timeSeconds)
    {
        return transform.position.y + SampleDisplacement(worldXZ, timeSeconds).y;
    }

    /// <summary>
    /// Returns the displaced world-space position of the water surface at the given XZ location and time.
    /// </summary>
    public Vector3 SampleWorldPosition(Vector2 worldXZ, float timeSeconds)
    {
        Vector3 basePosition = new Vector3(worldXZ.x, transform.position.y, worldXZ.y);
        return basePosition + SampleDisplacement(worldXZ, timeSeconds);
    }

    /// <summary>
    /// Estimates the world-space normal of the water surface using neighboring samples.
    /// </summary>
    public Vector3 SampleNormal(Vector2 worldXZ, float timeSeconds, float eps = 0.2f)
    {
        Vector3 center = SampleWorldPosition(worldXZ, timeSeconds);
        Vector3 sampleX = SampleWorldPosition(worldXZ + new Vector2(eps, 0f), timeSeconds);
        Vector3 sampleZ = SampleWorldPosition(worldXZ + new Vector2(0f, eps), timeSeconds);

        Vector3 tangentX = sampleX - center;
        Vector3 tangentZ = sampleZ - center;
        Vector3 normal = Vector3.Cross(tangentZ, tangentX);

        if (normal.sqrMagnitude < 0.000001f)
        {
            return Vector3.up;
        }

        return normal.normalized;
    }
}