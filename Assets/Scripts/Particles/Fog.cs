using UnityEngine;
using System.Collections.Generic;

public class FogVolume2D : MonoBehaviour
{
    [Header("Fog Volume")]
    [Min(1)]
    public int layerCount = 16;
    public float totalDepth = 2f;
    public Vector2 size = new Vector2(30, 15);

    [Header("Material")]
    public Material fogMaterialTemplate;

    [Header("Look")]
    public Color fogColor = new Color(0.9f, 0.95f, 1f, 0.12f);
    public float baseTiling = 1f;
    public Vector2 baseScrollSpeed = new Vector2(0.15f, 0.05f);

    [Header("Variation")]
    public float speedJitter = 0.05f;
    public float tilingJitter = 0.3f;
    public float alphaFalloff = 0.9f;

    [Header("Debug")]
    [Range(1, 256)]
    public int debugIntensity = 16;


    List<GameObject> layers = new List<GameObject>();

    void Start()
    {
        Rebuild();
    }

    // PUBLIC API
    public void SetIntensity(int newLayerCount)
    {
        newLayerCount = Mathf.Max(1, newLayerCount);

        if (newLayerCount == layerCount)
            return;

        layerCount = newLayerCount;
        Rebuild();
    }

    public void SetTotalDepth(float depth)
    {
        totalDepth = Mathf.Max(0.01f, depth);
        Rebuild();
    }

    void Rebuild()
    {
        ClearLayers();
        GenerateFogLayers();
    }

    void ClearLayers()
    {
        foreach (var l in layers)
        {
            if (l != null)
                Destroy(l);
        }
        layers.Clear();
    }

    void GenerateFogLayers()
    {
        float spacing =
            layerCount > 1 ? totalDepth / (layerCount - 1) : 0f;

        for (int i = 0; i < layerCount; i++)
        {
            float depth01 =
                layerCount > 1 ? i / (float)(layerCount - 1) : 0f;

            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = $"FogLayer_{i}";
            quad.transform.SetParent(transform, false);

            quad.transform.localPosition = new Vector3(
                0,
                0,
                -depth01 * totalDepth
            );

            quad.transform.localScale = size;

            var mat = new Material(fogMaterialTemplate);

            // Alpha falloff with depth
            Color c = fogColor;
            c.a *= Mathf.Lerp(1f, Mathf.Pow(alphaFalloff, layerCount), depth01);
            mat.SetColor("_Color", c);

            // Depth-based speed parallax (front = faster)
            float speedFactor = Mathf.Lerp(1.25f, 0.6f, depth01);
            Vector2 speed =
                baseScrollSpeed * speedFactor +
                Random.insideUnitCircle * speedJitter;

            mat.SetVector("_Speed", new Vector4(speed.x, speed.y, 0, 0));

            // Tiling variation
            mat.SetFloat(
                "_Tiling",
                baseTiling + Random.Range(-tilingJitter, tilingJitter)
            );

            // Per-layer noise offset
            mat.SetVector("_NoiseOffset", new Vector4(
                Random.value * 100f,
                Random.value * 100f,
                Random.value * 100f,
                Random.value * 100f
            ));

            quad.GetComponent<Renderer>().material = mat;

            layers.Add(quad);
        }
    }
    
#if UNITY_EDITOR
    void OnValidate()
    {
        if (!Application.isPlaying)
            return;

        SetIntensity(debugIntensity);
    }
#endif
}
