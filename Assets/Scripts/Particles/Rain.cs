using UnityEngine;

/// <summary>
/// Controls a rain particle system effect in the foreground of the scene.
/// Attach this to a GameObject with a ParticleSystem component.
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class Rain : MonoBehaviour
{
    [Header("Rain Settings")]
    [Tooltip("Number of rain particles emitted per second")]
    [SerializeField] private float emissionRate = 500f;
    
    [Tooltip("Lifetime of each rain drop in seconds")]
    [SerializeField] private float lifetime = 1.5f;
    
    [Tooltip("Speed of falling rain")]
    [SerializeField] private float fallSpeed = 10f;
    
    [Tooltip("Width of the rain spawn area")]
    [SerializeField] private float spawnWidth = 20f;
    
    [Tooltip("Height of the rain spawn area")]
    [SerializeField] private float spawnHeight = 2f;
    
    [Tooltip("Length/stretch of rain drops")]
    [SerializeField] private float dropLength = 0.5f;
    
    [Tooltip("Width of rain drops")]
    [SerializeField] private float dropWidth = 0.02f;
    
    [Tooltip("Color of the rain")]
    [SerializeField] private Color rainColor = new Color(0.7f, 0.8f, 1f, 0.5f);
    
    [Header("Wind Settings")]
    [Tooltip("Horizontal wind force affecting rain angle")]
    [SerializeField] private float windStrength = 0f;
    
    [Header("Intensity")]
    [Range(0f, 1f)]
    [Tooltip("Overall rain intensity (0 = no rain, 1 = full rain)")]
    [SerializeField] private float intensity = 1f;
    
    private ParticleSystem rainParticleSystem;
    private ParticleSystem.MainModule mainModule;
    private ParticleSystem.EmissionModule emissionModule;
    private ParticleSystem.ShapeModule shapeModule;
    private ParticleSystem.VelocityOverLifetimeModule velocityModule;
    
    private float baseEmissionRate;

    private void Awake()
    {
        rainParticleSystem = GetComponent<ParticleSystem>();
        InitializeParticleSystem();
    }

    private void InitializeParticleSystem()
    {
        // Cache modules
        mainModule = rainParticleSystem.main;
        emissionModule = rainParticleSystem.emission;
        shapeModule = rainParticleSystem.shape;
        velocityModule = rainParticleSystem.velocityOverLifetime;
        
        // Configure main module
        mainModule.simulationSpace = ParticleSystemSimulationSpace.World;
        mainModule.startLifetime = lifetime;
        mainModule.startSpeed = 0f; // We control speed via velocity over lifetime
        mainModule.startSize = dropWidth; // Width of the rain drop (length controlled by renderer lengthScale)
        mainModule.startColor = rainColor;
        mainModule.gravityModifier = 0f; // We handle gravity manually
        mainModule.maxParticles = 10000;
        
        // Configure emission
        baseEmissionRate = emissionRate;
        emissionModule.rateOverTime = emissionRate * intensity;
        
        // Configure shape (box emitter above the camera)
        shapeModule.shapeType = ParticleSystemShapeType.Box;
        shapeModule.scale = new Vector3(spawnWidth, spawnHeight, 1f);
        
        // Configure velocity for falling rain
        velocityModule.enabled = true;
        velocityModule.space = ParticleSystemSimulationSpace.World;
        velocityModule.x = windStrength;
        velocityModule.y = -fallSpeed;
        velocityModule.z = 0f;
        
        // Configure renderer for stretched billboards (rain effect)
        var renderer = rainParticleSystem.GetComponent<ParticleSystemRenderer>();
        if (renderer != null)
        {
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.velocityScale = 0f; // Disable velocity-based stretching
            renderer.lengthScale = dropLength; // Use dropLength to control visual length
        }
    }

    /// <summary>
    /// Sets the rain intensity (0 = no rain, 1 = full rain)
    /// </summary>
    public void SetIntensity(float newIntensity)
    {
        intensity = Mathf.Clamp01(newIntensity);
        emissionModule.rateOverTime = baseEmissionRate * intensity;
    }

    /// <summary>
    /// Sets the wind strength affecting rain angle
    /// </summary>
    public void SetWindStrength(float strength)
    {
        windStrength = strength;
        velocityModule.x = windStrength;
    }

    /// <summary>
    /// Sets the fall speed of rain drops
    /// </summary>
    public void SetFallSpeed(float speed)
    {
        fallSpeed = speed;
        velocityModule.y = -fallSpeed;
    }

    /// <summary>
    /// Sets the emission rate of rain particles
    /// </summary>
    public void SetEmissionRate(float rate)
    {
        emissionRate = rate;
        baseEmissionRate = rate;
        emissionModule.rateOverTime = baseEmissionRate * intensity;
    }

    /// <summary>
    /// Sets the color of the rain
    /// </summary>
    public void SetRainColor(Color color)
    {
        rainColor = color;
        mainModule.startColor = rainColor;
    }

    /// <summary>
    /// Sets the spawn area dimensions
    /// </summary>
    public void SetSpawnArea(float width, float height)
    {
        spawnWidth = width;
        spawnHeight = height;
        shapeModule.scale = new Vector3(spawnWidth, spawnHeight, 1f);
    }

    /// <summary>
    /// Starts the rain effect
    /// </summary>
    public void StartRain()
    {
        if (!rainParticleSystem.isPlaying)
        {
            rainParticleSystem.Play();
        }
    }

    /// <summary>
    /// Stops the rain effect
    /// </summary>
    public void StopRain()
    {
        if (rainParticleSystem.isPlaying)
        {
            rainParticleSystem.Stop();
        }
    }

    /// <summary>
    /// Checks if rain is currently active
    /// </summary>
    public bool IsRaining => rainParticleSystem.isPlaying && intensity > 0f;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (rainParticleSystem == null)
            rainParticleSystem = GetComponent<ParticleSystem>();
            
        if (rainParticleSystem != null && Application.isPlaying)
        {
            InitializeParticleSystem();
        }
    }
#endif
}
