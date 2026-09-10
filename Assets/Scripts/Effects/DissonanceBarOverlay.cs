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

    [Header("Debug")]
    [Tooltip("Live stack count. Editable in play mode to preview without the status effect.")]
    [SerializeField] private int stacks;

    Material _template;         // per-instance base, so bars animate independently
    Material _spillTemplate;
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
            _template = new Material(staticFill.material);
            staticFill.material = _template;
            staticFill.raycastTarget = false;
        }

        if (spillFill != null && spillFill.material != null)
        {
            _spillTemplate = new Material(spillFill.material);
            _spillTemplate.SetFloat("_Spill", 1f);
            spillFill.material = _spillTemplate;
            spillFill.raycastTarget = false;
            spillFill.enabled = true;
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

        mat.SetFloat(FillID, _displayFill);
        mat.SetFloat(ProximityID, p);

        Material spillMat = SpillMaterial;
        if (spillMat != null)
        {
            spillMat.SetFloat(FillID, _displayFill);
            spillMat.SetFloat(ProximityID, p);
            spillMat.SetFloat("_Spill", 1f);
            PushPadding(spillMat);
        }

        if (!_dying && stacks <= 0 && _displayFill <= 0.001f) SetVisible(false);
    }
}