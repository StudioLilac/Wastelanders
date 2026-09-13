using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Sits on the main camera. Each frame that the effect is active it:
///   1. Renders every registered ImpactSilhouette into a single-channel mask RT
///      using a command buffer (works in Built-in and URP, no renderer features).
///   2. Drives a camera-parented fullscreen quad that composites the mask into
///      the black-on-white impact frame.
///
/// Set <see cref="intensity"/> to 0 and the whole thing costs nothing -- no mask
/// render, quad disabled.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Impact/Impact Frame Controller")]
public class ImpactFrameController : MonoBehaviour
{
    static readonly int SilhouetteTexID = Shader.PropertyToID("_SilhouetteTex");
    static readonly int CutoffID        = Shader.PropertyToID("_Cutoff");
    static readonly int MaskTexID       = Shader.PropertyToID("_MaskTex");
    static readonly int FgColorID       = Shader.PropertyToID("_FgColor");
    static readonly int BgColorID       = Shader.PropertyToID("_BgColor");
    static readonly int IntensityID     = Shader.PropertyToID("_Intensity");
    static readonly int DilateID        = Shader.PropertyToID("_Dilate");
    static readonly int ThresholdID     = Shader.PropertyToID("_Threshold");
    static readonly int InvertID        = Shader.PropertyToID("_Invert");

    [Header("Shaders")]
    public Shader maskShader;   // Impact/Mask
    public Shader frameShader;  // Impact/ImpactFrame

    [Header("Drive these from the sequencer")]
    [Range(0f, 1f)] public float intensity = 0f;
    [Range(0f, 1f)] public float invert = 0f;

    [Header("Look")]
    public Color silhouetteColor = Color.black;
    public Color backgroundColor = Color.white;
    [Tooltip("Fattens the silhouette so thin details survive the hard threshold.")]
    [Range(0f, 10f)] public float dilatePixels = 1.5f;
    [Range(0.001f, 1f)] public float threshold = 0.2f;
    [Tooltip("Alpha below this is cut out of the silhouette. Raise it if soft " +
             "particle edges are bleeding into blobs.")]
    [Range(0f, 1f)] public float alphaCutoff = 0.1f;

    [Header("Performance")]
    [Tooltip("1 = full res. 2 gives a chunkier, more stylised edge for free.")]
    [Range(1, 4)] public int maskDownsample = 1;

    Camera _cam;
    RenderTexture _maskRT;
    Material _maskMat;
    Material _frameMat;
    CommandBuffer _cb;
    Transform _quad;
    MeshRenderer _quadRenderer;
    int _rtW, _rtH;

    public bool IsActive => intensity > 0.0001f;

    /// <summary>Convenience for the sequencer.</summary>
    public void SetIntensity(float v) => intensity = Mathf.Clamp01(v);

    void OnEnable()
    {
        _cam = GetComponent<Camera>();
        EnsureResources();

        Camera.onPreRender += OnBuiltinPreRender;
        RenderPipelineManager.beginCameraRendering += OnSrpBeginCameraRendering;
    }

    void OnDisable()
    {
        Camera.onPreRender -= OnBuiltinPreRender;
        RenderPipelineManager.beginCameraRendering -= OnSrpBeginCameraRendering;

        ReleaseResources();
    }

    // ------------------------------------------------------------------ setup

    void EnsureResources()
    {
        if (maskShader == null) maskShader = Shader.Find("Impact/Mask");
        if (frameShader == null) frameShader = Shader.Find("Impact/ImpactFrame");

        if (_maskMat == null && maskShader != null)
            _maskMat = new Material(maskShader) { hideFlags = HideFlags.HideAndDontSave };

        if (_frameMat == null && frameShader != null)
            _frameMat = new Material(frameShader) { hideFlags = HideFlags.HideAndDontSave };

        if (_cb == null)
            _cb = new CommandBuffer { name = "Impact Frame Mask" };

        EnsureQuad();
    }

    void EnsureQuad()
    {
        if (_quad != null) return;

        var go = new GameObject("~ImpactFrameQuad")
        {
            hideFlags = HideFlags.HideAndDontSave
        };
        go.transform.SetParent(transform, false);

        var mf = go.AddComponent<MeshFilter>();
        mf.sharedMesh = BuildQuad();

        _quadRenderer = go.AddComponent<MeshRenderer>();
        _quadRenderer.sharedMaterial = _frameMat;
        _quadRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _quadRenderer.receiveShadows = false;
        _quadRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        _quadRenderer.allowOcclusionWhenDynamic = false;

        // URP's 2D Renderer sorts MeshRenderers against sprites by sorting layer.
        // Pin the quad to the very top so sprites can never draw over the frame.
        var layers = SortingLayer.layers;
        if (layers.Length > 0)
            _quadRenderer.sortingLayerID = layers[layers.Length - 1].id;
        _quadRenderer.sortingOrder = short.MaxValue;

        _quadRenderer.enabled = false;
        _quad = go.transform;
    }

    static Mesh BuildQuad()
    {
        var m = new Mesh { name = "~ImpactFullscreenQuad", hideFlags = HideFlags.HideAndDontSave };
        m.vertices = new[]
        {
            new Vector3(-0.5f, -0.5f, 0f),
            new Vector3( 0.5f, -0.5f, 0f),
            new Vector3( 0.5f,  0.5f, 0f),
            new Vector3(-0.5f,  0.5f, 0f),
        };
        m.uv = new[] { Vector2.zero, Vector2.right, Vector2.one, Vector2.up };
        m.triangles = new[] { 0, 2, 1, 0, 3, 2 };
        // Oversized bounds: the quad lives inside the near plane and would
        // otherwise get frustum-culled on some aspect ratios.
        m.bounds = new Bounds(Vector3.zero, Vector3.one * 1000f);
        return m;
    }

    void ReleaseResources()
    {
        if (_maskRT != null)
        {
            _maskRT.Release();
            DestroyImmediate(_maskRT);
            _maskRT = null;
        }

        if (_quad != null)
        {
            if (Application.isPlaying) Destroy(_quad.gameObject);
            else DestroyImmediate(_quad.gameObject);
            _quad = null;
            _quadRenderer = null;
        }

        _cb?.Release();
        _cb = null;
    }

    void EnsureMaskRT()
    {
        int w = Mathf.Max(1, _cam.pixelWidth / maskDownsample);
        int h = Mathf.Max(1, _cam.pixelHeight / maskDownsample);

        if (_maskRT != null && _rtW == w && _rtH == h && _maskRT.IsCreated())
            return;

        if (_maskRT != null)
        {
            _maskRT.Release();
            DestroyImmediate(_maskRT);
        }

        var fmt = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.R8)
            ? RenderTextureFormat.R8
            : RenderTextureFormat.ARGB32;

        _maskRT = new RenderTexture(w, h, 0, fmt, RenderTextureReadWrite.Linear)
        {
            name = "~ImpactMask",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            antiAliasing = 1,
            hideFlags = HideFlags.HideAndDontSave
        };
        _maskRT.Create();

        _rtW = w;
        _rtH = h;
    }

    // ------------------------------------------------------------------ frame

    void LateUpdate()
    {
        if (_cam == null) _cam = GetComponent<Camera>();
        EnsureResources();

        bool active = IsActive && _frameMat != null && _maskMat != null;

        if (_quadRenderer != null)
            _quadRenderer.enabled = active;

        if (!active) return;

        FitQuadToFrustum();
        PushFrameMaterialParams();
    }

    void FitQuadToFrustum()
    {
        float dist = _cam.nearClipPlane + 0.01f;

        float h = _cam.orthographic
            ? _cam.orthographicSize * 2f
            : 2f * dist * Mathf.Tan(_cam.fieldOfView * 0.5f * Mathf.Deg2Rad);

        float w = h * _cam.aspect;

        // Slight overscan so no seam appears at the screen edge.
        _quad.localPosition = new Vector3(0f, 0f, dist);
        _quad.localRotation = Quaternion.identity;
        _quad.localScale = new Vector3(w * 1.02f, h * 1.02f, 1f);
    }

    void PushFrameMaterialParams()
    {
        _frameMat.SetTexture(MaskTexID, _maskRT);
        _frameMat.SetColor(FgColorID, silhouetteColor);
        _frameMat.SetColor(BgColorID, backgroundColor);
        _frameMat.SetFloat(IntensityID, Mathf.Clamp01(intensity));
        _frameMat.SetFloat(DilateID, dilatePixels);
        _frameMat.SetFloat(ThresholdID, threshold);
        _frameMat.SetFloat(InvertID, Mathf.Clamp01(invert));
    }

    void OnBuiltinPreRender(Camera cam)
    {
        if (cam == _cam) RenderMask();
    }

    void OnSrpBeginCameraRendering(ScriptableRenderContext ctx, Camera cam)
    {
        if (cam == _cam) RenderMask();
    }

    void RenderMask()
    {
        if (!IsActive || _maskMat == null || _cb == null) return;

        EnsureMaskRT();
        _maskMat.SetFloat(CutoffID, alphaCutoff);

        _cb.Clear();
        _cb.SetRenderTarget(_maskRT);
        _cb.ClearRenderTarget(true, true, Color.clear);
        _cb.SetViewProjectionMatrices(_cam.worldToCameraMatrix, _cam.projectionMatrix);

        var sources = ImpactSilhouette.Active;
        for (int i = 0; i < sources.Count; i++)
        {
            var src = sources[i];
            if (src == null || !src.contributeToMask || src.renderers == null) continue;

            for (int r = 0; r < src.renderers.Count; r++)
            {
                var rend = src.renderers[r];
                if (rend == null || !rend.enabled || !rend.gameObject.activeInHierarchy) continue;

                // Per-draw global. The mask shader does NOT declare this as a
                // material property, so the global is what actually binds.
                _cb.SetGlobalTexture(SilhouetteTexID, src.ResolveTexture(rend));
                _cb.DrawRenderer(rend, _maskMat, 0, 0);
            }
        }

        Graphics.ExecuteCommandBuffer(_cb);
    }
}
