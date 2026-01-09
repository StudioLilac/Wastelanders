using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class RuntimeFogNoise : MonoBehaviour
{
    [Header("Fog Material")]
    [Tooltip("Material using the Blizzard/ScrollingFog2D shader with a noise texture assigned")]
    public Material fogMaterialTemplate;

    [Header("Fog Look")]
    public Color fogColor = new Color(0.9f, 0.95f, 1f, 0.12f);
    public Vector2 scrollSpeed = new Vector2(0.15f, 0.05f);
    public float tiling = 1f;

    Material fogMatInstance;

    void Start()
    {
        if (fogMaterialTemplate == null)
        {
            Debug.LogError("Fog material template not assigned.", this);
            return;
        }

        var r = GetComponent<Renderer>();

        // Clone so each fog layer can differ
        fogMatInstance = new Material(fogMaterialTemplate);
        r.material = fogMatInstance;

        ApplyParameters();
    }

    void ApplyParameters()
    {
        fogMatInstance.SetColor("_Color", fogColor);
        fogMatInstance.SetVector("_Speed", new Vector4(scrollSpeed.x, scrollSpeed.y, 0, 0));
        fogMatInstance.SetFloat("_Tiling", tiling);
    }

#if UNITY_EDITOR
    // Allows live tuning in play mode
    void OnValidate()
    {
        if (fogMatInstance != null)
            ApplyParameters();
    }
#endif
}