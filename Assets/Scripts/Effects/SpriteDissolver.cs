using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Dematerialises a sprite and shakes it while it goes.
///
/// The shake ENVELOPE ramps up rather than decaying: the character rattles
/// harder as it comes apart, which reads as the thing losing cohesion. A
/// decaying shake reads as a single impact, which is the wrong beat here.
///
/// Materials are swapped for a shared dissolve material and driven per-renderer
/// through a MaterialPropertyBlock, so nothing is instanced and the sprite's own
/// _MainTex binding is preserved.
/// </summary>
[AddComponentMenu("Impact/Sprite Dissolver")]
public class SpriteDissolver : MonoBehaviour
{
    static readonly int ProgressID  = Shader.PropertyToID("_Progress");
    static readonly int DirectionID = Shader.PropertyToID("_Direction");
    static readonly int ExtentsID   = Shader.PropertyToID("_Extents");

    [Header("Targets")]
    [Tooltip("Leave empty to collect every SpriteRenderer under this object.")]
    public SpriteRenderer[] renderers;
    [Tooltip("Material using Impact/SpriteDissolvePixel.")]
    public Material dissolveMaterial;

    [Header("Dissolve")]
    public float duration = 1.2f;
    [Tooltip("Progress over normalised time. Ease-in holds the silhouette a beat " +
             "before it starts shedding.")]
    public AnimationCurve progress = new AnimationCurve(
        new Keyframe(0f, 0f, 0f, 0.35f), new Keyframe(1f, 1f, 2.2f, 0f));

    [Header("Shake")]
    public float shakeAmplitude = 0.09f;
    [Tooltip("Wobbles per second. 20-30 rattles; below 10 is a sway.")]
    public float shakeFrequency = 26f;
    [Tooltip("Strength over normalised time. Ramps UP -- it's falling apart, " +
             "not getting hit.")]
    public AnimationCurve shakeEnvelope = new AnimationCurve(
        new Keyframe(0f, 0f, 0f, 0.2f), new Keyframe(0.7f, 0.65f),
        new Keyframe(1f, 1f, 2.5f, 0f));
    [Tooltip("Vertical shake as a fraction of horizontal. Keep it under 1 -- pure " +
             "circular jitter reads as floating.")]
    [Range(0f, 1f)] public float verticalRatio = 0.55f;
    [Tooltip("0 = off. Set to your PPU to keep the shake on the pixel grid.")]
    public float snapPixelsPerUnit = 0f;

    [Header("Finish")]
    public bool deactivateOnFinish = true;
    public UnityEvent onFinished;

    Material[] _originalMaterials;
    Vector3 _basePosition;
    bool _running;

    public bool IsRunning => _running;

    void Reset() => renderers = GetComponentsInChildren<SpriteRenderer>(true);

    /// <summary>Sweeps along +X by default.</summary>
    public void Play() => StartCoroutine(Routine(Vector2.right));

    /// <summary>
    /// Sweep the dissolve along the direction of the blow -- pass
    /// (victim.position - attacker.position).
    /// </summary>
    public Coroutine Play(Vector2 sweepDirection) => StartCoroutine(Routine(sweepDirection));

    public IEnumerator Routine(Vector2 sweepDirection)
    {
        if (_running) yield break;
        if (dissolveMaterial == null)
        {
            Debug.LogError("[SpriteDissolver] No dissolveMaterial assigned.", this);
            yield break;
        }

        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<SpriteRenderer>(true);

        _running = true;
        _basePosition = transform.localPosition;

        SwapMaterials(sweepDirection);

        float seedX = Random.value * 100f;
        float seedY = Random.value * 100f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            SetProgress(progress.Evaluate(t));
            ApplyShake(elapsed, shakeEnvelope.Evaluate(t));

            yield return null;
        }

        SetProgress(1f);
        transform.localPosition = _basePosition;
        _running = false;

        onFinished?.Invoke();
        if (deactivateOnFinish) gameObject.SetActive(false);
    }

    /// <summary>Puts the original materials back. For a retry or a pooled character.</summary>
    public void RestoreMaterials()
    {
        if (_originalMaterials == null) return;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;
            renderers[i].sharedMaterial = _originalMaterials[i];
            renderers[i].SetPropertyBlock(null);
        }

        _originalMaterials = null;
        transform.localPosition = _basePosition;
    }

    // -------------------------------------------------------------------------

    void SwapMaterials(Vector2 sweep)
    {
        _originalMaterials = new Material[renderers.Length];
        Vector2 dir = sweep.sqrMagnitude < 1e-6f ? Vector2.right : sweep.normalized;

        var mpb = new MaterialPropertyBlock();

        for (int i = 0; i < renderers.Length; i++)
        {
            var r = renderers[i];
            if (r == null) continue;

            _originalMaterials[i] = r.sharedMaterial;
            r.sharedMaterial = dissolveMaterial;

            // Read first: the SpriteRenderer keeps _MainTex in its own block.
            r.GetPropertyBlock(mpb);
            mpb.SetVector(DirectionID, new Vector4(dir.x, dir.y, 0f, 0f));

            Vector2 extents = r.sprite != null
                ? (Vector2)r.sprite.bounds.extents
                : Vector2.one * 0.5f;
            mpb.SetVector(ExtentsID, new Vector4(extents.x, extents.y, 0f, 0f));
            mpb.SetFloat(ProgressID, 0f);

            r.SetPropertyBlock(mpb);
        }
    }

    void SetProgress(float value)
    {
        var mpb = new MaterialPropertyBlock();
        foreach (var r in renderers)
        {
            if (r == null) continue;
            r.GetPropertyBlock(mpb);
            mpb.SetFloat(ProgressID, value);
            r.SetPropertyBlock(mpb);
        }
    }

    void ApplyShake(float elapsed, float envelope)
    {
        float phase = elapsed * shakeFrequency;

        // Perlin wanders continuously. A raw sine on one axis is a slide, not a
        // shake, and per-frame Random is static rather than motion.
        float x = (Mathf.PerlinNoise(seedOffset + phase, 0f) * 2f - 1f);
        float y = (Mathf.PerlinNoise(0f, seedOffset + phase) * 2f - 1f) * verticalRatio;

        Vector3 offset = new Vector3(x, y, 0f) * (shakeAmplitude * envelope);
        Vector3 p = _basePosition + offset;

        if (snapPixelsPerUnit > 0f)
        {
            p.x = Mathf.Round(p.x * snapPixelsPerUnit) / snapPixelsPerUnit;
            p.y = Mathf.Round(p.y * snapPixelsPerUnit) / snapPixelsPerUnit;
        }

        transform.localPosition = p;
    }

    const float seedOffset = 13.7f;
}

/// <summary>
/// Aiming helpers for cone-shaped particle systems.
/// </summary>
public static class VfxAim
{
    /// <summary>
    /// Points a ParticleSystem's local +Z along a 2D direction.
    ///
    /// Cone shapes emit along local +Z, which faces into the screen in a 2D
    /// scene, so an unrotated cone emits nothing you can see. LookRotation is
    /// used rather than composing Eulers because local +Z is exactly what its
    /// forward argument sets -- there's no multiplication order to get backwards.
    /// </summary>
    public static void AimCone(ParticleSystem ps, Vector2 direction, float upwardBias = 0f)
    {
        if (ps == null) return;

        Vector2 d = direction.sqrMagnitude < 1e-6f ? Vector2.right : direction.normalized;
        if (!Mathf.Approximately(upwardBias, 0f))
            d = (d + Vector2.up * upwardBias).normalized;

        ps.transform.rotation = Quaternion.LookRotation(new Vector3(d.x, d.y, 0f), Vector3.back);
    }

    /// <summary>
    /// Aims the spew away from whoever landed the blow, with a slight lift so the
    /// debris arcs instead of spraying flat.
    /// </summary>
    public static void AimConeAwayFrom(ParticleSystem ps, Transform attacker, Transform victim,
                                       float upwardBias = 0.3f)
    {
        if (ps == null || attacker == null || victim == null) return;
        AimCone(ps, victim.position - attacker.position, upwardBias);
    }

    /// <summary>Moves the system to the victim and aims it, then plays.</summary>
    public static void SpewFrom(ParticleSystem ps, Transform attacker, Transform victim,
                                float upwardBias = 0.3f)
    {
        if (ps == null || victim == null) return;
        ps.transform.position = victim.position;
        AimConeAwayFrom(ps, attacker, victim, upwardBias);
        ps.Play(true);
    }
}
