using UnityEngine;

public class CrosshairGlowEffect : MonoBehaviour
{
    [Header("Glow Settings")]
    [SerializeField] private Color glowColorInner = new Color(1f, 0.85f, 0.3f, 0.6f);
    [SerializeField] private Color glowColorOuter = new Color(1f, 0.6f, 0.1f, 0f);
    [SerializeField] private float glowPadding = 0.3f;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseIntensity = 0.2f;
    [SerializeField] private float breatheSpeed = 1.5f;
    [SerializeField] private float breatheAmount = 0.05f;
    
    [Header("Sorting")]
    [SerializeField] private string sortingLayerName = "Default";
    [SerializeField] private int sortingOrder = -1;
    
    private EntityClass entity;
    private SpriteRenderer entityRenderer;
    private SpriteRenderer glowRenderer;
    private Material glowMaterial;
    private float timeOffset;
    private Vector2 baseSize;
    
    private void Awake()
    {
        FindEntity();
        timeOffset = Random.Range(0f, 100f);
    }
    
    private void Start()
    {
        if (entity != null)
        {
            SetupGlow();
        }
    }
    
    private void FindEntity()
    {
        Transform current = transform.parent;
        
        while (current != null)
        {
            entity = current.GetComponent<EntityClass>();
            if (entity != null)
            {
                entityRenderer = current.GetComponent<SpriteRenderer>();
                if (entityRenderer == null)
                    entityRenderer = current.GetComponentInChildren<SpriteRenderer>();
                return;
            }
            current = current.parent;
        }
        
        Debug.LogWarning("CylinderGlowEffect: No EntityClass found in parent hierarchy.");
    }
    
    private void SetupGlow()
    {
        // Create glow sprite object
        GameObject glowObj = new GameObject("GlowEffect");
        glowObj.transform.SetParent(transform);
        glowObj.transform.localPosition = Vector3.zero;
        glowObj.transform.localRotation = Quaternion.identity;
        
        glowRenderer = glowObj.AddComponent<SpriteRenderer>();
        glowRenderer.sprite = CreateSoftGlowSprite(64);
        glowRenderer.sortingLayerName = sortingLayerName;
        glowRenderer.sortingOrder = sortingOrder;
        
        // Create material for color control
        glowMaterial = new Material(Shader.Find("Sprites/Default"));
        glowRenderer.material = glowMaterial;
        
        UpdateGlowSize();
    }
    
    private Sprite CreateSoftGlowSprite(int resolution)
    {
        Texture2D texture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        
        Vector2 center = new Vector2(resolution / 2f, resolution / 2f);
        float maxRadius = resolution / 2f;
        
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float normalizedDist = Mathf.Clamp01(distance / maxRadius);
                
                // Soft falloff using smoothstep-like curve
                float alpha = 1f - Mathf.SmoothStep(0f, 1f, normalizedDist);
                alpha = Mathf.Pow(alpha, 1.5f); // Extra softness
                
                Color color = Color.Lerp(glowColorOuter, glowColorInner, alpha);
                color.a *= alpha;
                
                texture.SetPixel(x, y, color);
            }
        }
        
        texture.Apply();
        
        return Sprite.Create(
            texture,
            new Rect(0, 0, resolution, resolution),
            new Vector2(0.5f, 0.5f),
            resolution
        );
    }
    
    private void UpdateGlowSize()
    {
        if (entityRenderer == null || entityRenderer.sprite == null) return;
        
        Bounds bounds = entityRenderer.sprite.bounds;
        baseSize = new Vector2(
            bounds.size.x * entityRenderer.transform.lossyScale.x + glowPadding * 2f,
            bounds.size.y * entityRenderer.transform.lossyScale.y + glowPadding * 2f
        );
        
        glowRenderer.transform.localScale = new Vector3(baseSize.x, baseSize.y, 1f);
    }
    
    private void Update()
    {
        if (glowRenderer == null) return;
        
        float time = Time.time + timeOffset;
        
        // Pulse effect on alpha/color
        float pulse = Mathf.Sin(time * pulseSpeed) * 0.5f + 0.5f;
        Color currentColor = Color.Lerp(glowColorInner, glowColorOuter, pulse * pulseIntensity);
        currentColor.a = glowColorInner.a * (1f - pulse * pulseIntensity * 0.5f);
        glowMaterial.color = currentColor;
        
        // Breathing effect on scale
        float breathe = Mathf.Sin(time * breatheSpeed) * breatheAmount;
        Vector3 scale = new Vector3(
            baseSize.x * (1f + breathe),
            baseSize.y * (1f + breathe),
            1f
        );
        glowRenderer.transform.localScale = scale;
    }
    
    public void RefreshSize()
    {
        UpdateGlowSize();
    }
    
    private void OnDestroy()
    {
        if (glowMaterial != null)
            Destroy(glowMaterial);
    }
}