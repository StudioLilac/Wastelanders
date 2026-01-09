using UnityEngine;

public class FogVolume2D : MonoBehaviour
{
    [Header("Fog Volume")]
    public int layerCount = 16;
    public float depthSpacing = 0.1f;
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

    void Start()
    {
        GenerateFogLayers();
    }

    void GenerateFogLayers()
    {
        for (int i = 0; i < layerCount; i++)
        {
            float depth01 = i / (float)(layerCount - 1);

            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = $"FogLayer_{i}";
            quad.transform.SetParent(transform, false);

            quad.transform.localPosition = new Vector3(
                0,
                0,
                -i * depthSpacing
            );

            quad.transform.localScale = size;

            var mat = new Material(fogMaterialTemplate);

            // Color & depth fade
            Color c = fogColor;
            c.a *= Mathf.Pow(alphaFalloff, i);
            mat.SetColor("_Color", c);

            // Speed variation
            Vector2 speed = baseScrollSpeed +
                Random.insideUnitCircle * speedJitter;
            mat.SetVector("_Speed", new Vector4(speed.x, speed.y, 0, 0));

            // Tiling variation
            mat.SetFloat("_Tiling",
                baseTiling + Random.Range(-tilingJitter, tilingJitter));

            // Unique noise offset per layer (IMPORTANT)
            mat.SetVector("_NoiseOffset", new Vector4(
                Random.value * 100f,
                Random.value * 100f,
                Random.value * 100f,
                0
            ));

            quad.GetComponent<Renderer>().material = mat;
        }
    }
}
