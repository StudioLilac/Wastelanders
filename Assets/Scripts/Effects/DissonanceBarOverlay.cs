using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drives the "UI/Dissonance Static" material on a health bar.
///
/// Shares the health bar's existing Slider — it only READS maxValue and
/// value, never writes, and never touches fillRect. No second Slider is
/// needed and there is no conflict with the red fill.
///
/// Hierarchy: put the static Image inside the same mask as the red fill,
/// as a sibling ordered AFTER it, with stretch anchors and zero offsets.
/// That puts it in exactly the 0..1 space the fill uses, so the two
/// fronts line up. Image Type must be Simple, sprite empty, Raycast
/// Target off.
/// </summary>
[AddComponentMenu("UI/Dissonance Bar Overlay")]
public class DissonanceBarOverlay : MonoBehaviour
{
    static readonly int FillID = Shader.PropertyToID("_Fill");
    static readonly int ProximityID = Shader.PropertyToID("_Proximity");
    static readonly int PadXID = Shader.PropertyToID("_PadX");
    static readonly int PadYID = Shader.PropertyToID("_PadY");
    static readonly int NoiseID = Shader.PropertyToID("_NoiseScale");
    static readonly int ScanRowsID = Shader.PropertyToID("_ScanRows");
    static readonly int SpillID = Shader.PropertyToID("_Spill");
    static readonly int AspectID = Shader.PropertyToID("_BarAspect");

    [Header("References")]
    [Tooltip("The health bar's existing Slider. Read-only use: maxValue and value.")]
    [SerializeField] private Slider healthSlider;

    [Tooltip("Plain (Simple) Image over the fill, using a UI/Dissonance Static material.")]
    [SerializeField] private Image staticFill;

    [Tooltip("Optional. Second Image OUTSIDE the fill mask, larger than the bar, using the " +
             "same shader with Spill Mode on. Draws detached blocks and tear streaks beyond " +
             "the bar edges. Leave empty to skip the spill layer entirely.")]
    [SerializeField] private Image spillFill;

    [Tooltip("The rect the bar's fill occupies. Used to work out how much bigger the spill " +
             "rect is. Defaults to staticFill's own rect.")]
    [SerializeField] private RectTransform barRect;

    [Header("Feel")]
    [Tooltip("Seconds for the corruption front to creep to a newly gained stack count.")]
    [SerializeField] private float creepDuration = 0.45f;

    [Tooltip("Extra aggression spike when stacks are gained, on top of proximity.")]
    [Range(0f, 1f)][SerializeField] private float gainSurge = 0.5f;

    [SerializeField] private float surgeDecay = 1.6f;

    [Tooltip("Proximity below which the bar stays calm. Ratio of stacks to current health.")]
    [Range(0f, 1f)][SerializeField] private float proximityFloor = 0.15f;

    public enum CellSizeMode
    {
        /// <summary>Project the rect to screen space and size cells in real pixels.
        /// Correct for world space canvases, where lossyScale is world units.</summary>
        ScreenPixels,
        /// <summary>Size cells in the rect's own units (90x18 here). Stable and
        /// camera-independent, but apparent cell size changes as you zoom.</summary>
        RectUnits,
        /// <summary>Leave _NoiseScale and _ScanRows alone; drive them from the material.</summary>
        Manual
    }

    [Header("Cell Sizing")]
    [Tooltip("How the noise grid is sized. ScreenPixels is correct for a world space " +
             "canvas: lossyScale there is world units, not pixels, so naive scaling " +
             "produces a 2-cell grid that strobes instead of reading as static.")]
    [SerializeField] private CellSizeMode cellSizeMode = CellSizeMode.ScreenPixels;

    [Tooltip("One static cell, in screen pixels (ScreenPixels) or rect units (RectUnits). " +
             "3-5 px reads as TV snow.")]
    [Range(0.5f, 16f)][SerializeField] private float cellSize = 4f;

    [Tooltip("Spacing between scanlines, same units as cellSize. Too tight aliases into " +
             "black banding. On a bar this short, consider _Scanline 0 instead.")]
    [Range(1f, 24f)][SerializeField] private float scanlineSpacing = 5f;

    [Tooltip("Clamps to stop degenerate grids on very small or very large bars.")]
    [SerializeField] private Vector2 cellCountClamp = new Vector2(8f, 400f);

    [Tooltip("Read-only: the grid actually being used. If this reads 8 x 1, measurement " +
             "failed and every fine-grain slider (chroma, subpixel, separation) will look dead.")]
    [SerializeField] private Vector2 debugCellCounts;

    [Header("Debug")]
    [Tooltip("Copy the material ASSET's properties onto the live material every frame, so " +
             "you can tune the material in the inspector during play and see it immediately. " +
             "Turn OFF for shipping: it costs a copy per bar per frame and makes every bar " +
             "share the asset's look.")]
    [SerializeField] private bool liveEditMaterial;

    [Tooltip("Live stack count. Editable in play mode to preview without the status effect.")]
    [SerializeField] private int stacks;

    Material _template;         // per-instance base, so bars animate independently
    Material _spillTemplate;
    bool _warnedMeasure;
    Material _sourceAsset;      // the shared asset, kept for live editing
    Material _spillSourceAsset;
    float _displayFill;
    float _targetFill;
    float _surge;
    bool _dying;

    public int Stacks => stacks;

    /// <summary>
    /// The material actually submitted for rendering.
    ///
    /// This matters: if the Image sits inside a UI Mask, Unity's stencil
    /// machinery renders a COPY of our material, and property writes to the
    /// original never reach the screen — the bar would sit frozen at zero
    /// with no error. materialForRendering returns that copy when a mask is
    /// present and the material itself when it isn't, so it is correct in
    /// both cases. We rewrite every frame, so mask rebuilds self-correct.
    /// </summary>
    Material TargetMaterial => staticFill != null ? staticFill.materialForRendering : null;
    Material SpillMaterial => spillFill != null ? spillFill.materialForRendering : null;

    void Awake()
    {
        if (staticFill != null && staticFill.material != null)
        {
            _sourceAsset = staticFill.material;
            _template = new Material(staticFill.material);
            staticFill.material = _template;
            staticFill.raycastTarget = false;
        }

        if (spillFill != null && spillFill.material != null)
        {
            _spillSourceAsset = spillFill.material;
            _spillTemplate = new Material(spillFill.material);
            _spillTemplate.SetFloat("_Spill", 1f);
            spillFill.material = _spillTemplate;
            spillFill.raycastTarget = false;
        }

        if (barRect == null && staticFill != null)
            barRect = staticFill.rectTransform;

        SetVisible(false);
    }

    void OnDestroy()
    {
        if (_template != null) Destroy(_template);
        if (_spillTemplate != null) Destroy(_spillTemplate);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        // Lets you scrub the Debug stack count in play mode and watch the bar react.
        if (Application.isPlaying && staticFill != null) SetStacks(stacks);
    }
#endif

    /// <summary>Called by the Dissonance status effect whenever its stack count changes.</summary>
    public void SetStacks(int newStacks)
    {
        int previous = stacks;
        stacks = Mathf.Max(0, newStacks);

        if (stacks > previous) _surge = Mathf.Max(_surge, gainSurge);

        if (stacks <= 0 && !_dying) { Clear(); return; }

        SetVisible(true);
    }

    /// <summary>Corruption removed. Snaps the bar back to clean.</summary>
    public void Clear()
    {
        stacks = 0;
        _targetFill = 0f;
        _displayFill = 0f;
        _surge = 0f;
        _dying = false;
        SetVisible(false);
    }

    /// <summary>
    /// Call from the death path. Slams the bar to full corruption so the kill
    /// reads as the static swallowing the last of the health.
    /// </summary>
    public void TriggerLethal()
    {
        _dying = true;
        _surge = 1f;
        SetVisible(true);
    }

    void SetVisible(bool visible)
    {
        if (staticFill != null && staticFill.enabled != visible)
            staticFill.enabled = visible;
        if (spillFill != null && spillFill.enabled != visible)
            spillFill.enabled = visible;
    }

    // UGUI writes the active mask configuration into the material it renders
    // with. A blanket CopyPropertiesFromMaterial overwrites those, which
    // silently disables the mask and leaves the Image drawing its own
    // unclipped rect. So: save them, copy, put them back.
    static readonly string[] StencilProps =
    {
        "_Stencil", "_StencilComp", "_StencilOp", "_StencilReadMask",
        "_StencilWriteMask", "_ColorMask"
    };

    static readonly float[] StencilCache = new float[6];

    static void CopyTunables(Material dst, Material src)
    {
        if (dst == null || src == null) return;

        for (int i = 0; i < StencilProps.Length; i++)
            StencilCache[i] = dst.HasProperty(StencilProps[i]) ? dst.GetFloat(StencilProps[i]) : -1f;

        dst.CopyPropertiesFromMaterial(src);

        for (int i = 0; i < StencilProps.Length; i++)
            if (StencilCache[i] >= 0f && dst.HasProperty(StencilProps[i]))
                dst.SetFloat(StencilProps[i], StencilCache[i]);
    }

    /// <summary>
    /// Size the noise grid so cells land at a sensible on-screen size.
    ///
    /// Cell counts are meaningless on their own: 90 cells across a bar that is
    /// 90 units wide but drawn at 0.05 scale in world space is nonsense either
    /// way you slice it. ScreenPixels mode projects the rect's corners through
    /// the canvas camera to get its true pixel footprint, which is the only
    /// measurement that survives a world space canvas.
    ///
    /// Both images use bar-space cells, so both get the same values.
    /// </summary>
    void PushCellSizing(Material mat)
    {
        if (cellSizeMode == CellSizeMode.Manual || mat == null || barRect == null) return;

        Vector2 size = barRect.rect.size;

        if (cellSizeMode == CellSizeMode.ScreenPixels)
        {
            Vector2 screen = MeasureScreenSize();
            // A world space canvas often has no camera to project through, and
            // WorldToScreenPoint(null, ...) hands back raw world coordinates.
            // Anything implausibly small is that failure, not a tiny bar.
            if (screen.x >= 8f && screen.y >= 2f) size = screen;
            else if (!_warnedMeasure)
            {
                _warnedMeasure = true;
                Debug.LogWarning($"[DissonanceBarOverlay] Could not measure '{name}' in screen " +
                    "pixels (no canvas camera?). Falling back to RectUnits. Assign the canvas " +
                    "Event Camera, or set Cell Size Mode to RectUnits and tune cellSize.", this);
            }
        }

        if (size.x < 0.01f || size.y < 0.01f) return;

        float unit = Mathf.Max(cellSize, 0.01f);
        float cellsX = Mathf.Clamp(size.x / unit, cellCountClamp.x, cellCountClamp.y);
        float cellsY = Mathf.Clamp(size.y / unit, 1f, cellCountClamp.y);
        mat.SetVector(NoiseID, new Vector4(cellsX, cellsY, 0f, 0f));
        debugCellCounts = new Vector2(Mathf.Round(cellsX), Mathf.Round(cellsY));

        float rows = Mathf.Clamp(size.y / Mathf.Max(scanlineSpacing, 0.01f), 1f, 64f);
        mat.SetFloat(ScanRowsID, rows);

        // Sparks need real proportions to stay round: bar UV is anisotropic.
        Rect r = barRect.rect;
        if (r.height > 0.01f)
            mat.SetFloat(AspectID, Mathf.Clamp(r.width / r.height, 0.1f, 40f));
    }

    /// <summary>The bar's footprint in real screen pixels, camera and canvas aware.</summary>
    Vector2 MeasureScreenSize()
    {
        Canvas canvas = staticFill != null ? staticFill.canvas : null;
        Camera cam = null;
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            cam = canvas.worldCamera != null ? canvas.worldCamera : Camera.main;

        var corners = new Vector3[4];
        barRect.GetWorldCorners(corners);

        Vector2 bl = RectTransformUtility.WorldToScreenPoint(cam, corners[0]);
        Vector2 tr = RectTransformUtility.WorldToScreenPoint(cam, corners[2]);

        return new Vector2(Mathf.Abs(tr.x - bl.x), Mathf.Abs(tr.y - bl.y));
    }

    /// <summary>
    /// The spill Image is bigger than the bar, so it needs to know where the
    /// bar sits inside its own rect. Computed from the two RectTransforms, so
    /// you can resize either in the editor without retuning anything.
    /// Assumes the two rects share a centre.
    /// </summary>
    void PushPadding(Material mat)
    {
        if (mat == null || spillFill == null || barRect == null) return;

        Rect spill = spillFill.rectTransform.rect;
        Rect bar = barRect.rect;

        float padX = spill.width > 0.001f ? Mathf.Clamp01((spill.width - bar.width) * 0.5f / spill.width) : 0f;
        float padY = spill.height > 0.001f ? Mathf.Clamp01((spill.height - bar.height) * 0.5f / spill.height) : 0f;

        mat.SetFloat(PadXID, padX);
        mat.SetFloat(PadYID, padY);
    }

    void LateUpdate()
    {
        if (staticFill == null || !staticFill.enabled || healthSlider == null) return;

        Material mat = TargetMaterial;
        if (mat == null) return;

        float maxHealth = Mathf.Max(1f, healthSlider.maxValue);
        float health = Mathf.Max(0f, healthSlider.value);

        // Front position, in the same units the health fill uses.
        _targetFill = _dying ? 1f : Mathf.Clamp01(stacks / maxHealth);

        _displayFill = creepDuration <= 0f
            ? _targetFill
            : Mathf.MoveTowards(_displayFill, _targetFill, Time.deltaTime / creepDuration);

        // How close the two fronts are to touching. At 1 the next stack kills.
        float proximity = _dying ? 1f : Mathf.Clamp01(stacks / Mathf.Max(1f, health));
        proximity = Mathf.InverseLerp(proximityFloor, 1f, proximity);

        _surge = Mathf.MoveTowards(_surge, 0f, Time.deltaTime * surgeDecay);

        float p = Mathf.Clamp01(proximity + _surge);

        if (liveEditMaterial && _sourceAsset != null)
            CopyTunables(mat, _sourceAsset);

        mat.SetFloat(FillID, _displayFill);
        mat.SetFloat(ProximityID, p);
        mat.SetFloat(SpillID, 0f);
        PushCellSizing(mat);

        Material spillMat = SpillMaterial;
        if (spillMat != null)
        {
            if (liveEditMaterial && _spillSourceAsset != null)
                CopyTunables(spillMat, _spillSourceAsset);

            spillMat.SetFloat(FillID, _displayFill);
            spillMat.SetFloat(ProximityID, p);
            spillMat.SetFloat(SpillID, 1f);
            PushCellSizing(spillMat);
            PushPadding(spillMat);
        }

        if (!_dying && stacks <= 0 && _displayFill <= 0.001f) SetVisible(false);
    }
}