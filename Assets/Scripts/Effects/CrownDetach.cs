using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Detaches the crown at the kill, drives an authored arc through the impact
/// frame, then drops it with a hand-integrated ballistic fall.
///
/// ONE COORDINATE, used everywhere: _artPos, the world position of the crown
/// ARTWORK'S pivot point. The sprite is a character-sized canvas with the art up
/// at head height, so the transform origin sits well below the drawing. Poses
/// are applied as
///
///     transform.position = _artPos - rotation * pivot
///
/// which makes the art turn in place. Nothing in this class ever reads
/// transform.position back -- mixing the two representations is what makes the
/// crown drift.
///
/// Phases: ARC (authored, scaled time) -> FALL (ballistic) -> REST.
///
/// Wire to the sequencer:  onImpact -> Detach(),  onSettle -> Release()
/// </summary>
[AddComponentMenu("Impact/Crown Detach")]
public class CrownDetach : MonoBehaviour
{
    [Header("References")]
    public GameObject crownPrefab;
    [Tooltip("Where the crown spawns. If the crown sprite matches the character " +
             "sprite's size and alignment, this is just the character.")]
    public Transform anchor;

    [Tooltip("The crown artwork's pivot, in local units from the sprite's " +
             "transform origin. Everything rotates about this point.")]
    public Vector2 rotationPivot = Vector2.zero;

    [Header("Facing")]
    [Tooltip("Knockback direction: +1 = knocked toward +X. Set from the kill " +
             "with SetFacing(sign(victim.x - attacker.x)).")]
    public float facing = 1f;

    [Header("Owner sprite swap")]
    [Tooltip("Preferred. An Animator rewrites SpriteRenderer.sprite every frame, " +
             "so a direct assignment gets stomped.")]
    public Animator ownerAnimator;
    public AnimatorOverrideController overrideController;

    [Header("Arc")]
    [Tooltip("SCALED seconds. The budget from contact to settle is about 0.07s " +
             "with default sequencer timings.")]
    public float arcDuration = 0.08f;
    [Tooltip("How far the crown lags BEHIND the knockback, in units.")]
    public float lagDistance = 0.35f;
    [Tooltip("Upward pop. This is what separates the crown from the head " +
             "silhouette in the impact frame -- don't zero it.")]
    public float popHeight = 0.45f;
    [Tooltip("Degrees. Positive is counter-clockwise: the crown's axis leans " +
             "toward the attacker, the near rim of the base dips down.")]
    public float tiltDegrees = 22f;

    public AnimationCurve arcX = new AnimationCurve(
        new Keyframe(0f, 0f, 0f, 3f), new Keyframe(1f, 1f, 0f, 0f));
    public AnimationCurve arcY = new AnimationCurve(
        new Keyframe(0f, 0f, 0f, 3.2f), new Keyframe(0.55f, 1f, 0f, 0f),
        new Keyframe(1f, 0.72f, -1.1f, -1.1f));
    public AnimationCurve arcRotation = new AnimationCurve(
        new Keyframe(0f, 0f, 0f, 2.6f), new Keyframe(1f, 1f, 0.3f, 0f));

    [Header("Release")]
    [Tooltip("Off = freeze at the end of the arc. Useful for tuning the pose.")]
    public bool fallOnRelease = true;
    [Range(0f, 2f)] public float releaseVelocityScale = 1f;
    [Tooltip("Added on release. X is signed by facing. The arc ends nearly still, " +
             "so without this the crown drops straight down.")]
    public Vector2 releaseVelocity = new Vector2(-0.8f, 0.4f);
    [Tooltip("Tumble added on release, degrees/sec, signed by facing.")]
    public float releaseSpin = 140f;

    [Header("Fall")]
    [Tooltip("Units per second squared. 25-40 reads well at typical 2D scales.")]
    public float gravity = 30f;
    [Range(0f, 0.9f)] public float bounciness = 0.45f;
    [Tooltip("Horizontal speed lost per bounce.")]
    [Range(0f, 1f)] public float horizontalFriction = 0.35f;
    [Tooltip("Spin retained through each bounce.")]
    [Range(0f, 1f)] public float spinRetention = 0.55f;
    public int maxBounces = 3;
    [Tooltip("Upward speed below which a bounce counts as landing, so it doesn't " +
             "micro-hop forever.")]
    public float restThreshold = 1.2f;

    [Header("Ground")]
    [Tooltip("Optional. Leave empty to derive the floor from the crown sprite's " +
             "own bottom edge at spawn -- which works because the sprite matches " +
             "the character's size and alignment.")]
    public Transform groundOverride;
    [Tooltip("Resting height of the crown's pivot above the ground line. With the " +
             "pivot at the art's base this is 0.")]
    public float restOffsetY = 0f;

    [Header("Mask")]
    public bool addSilhouette = true;

    [Header("Events")]
    public UnityEvent onDetach;
    public UnityEvent onBounce;
    public UnityEvent onRest;

    enum Phase { Idle, Arc, Fall, Rest }

    Phase _phase = Phase.Idle;
    GameObject _crown;
    Transform _crownT;

    Vector3 _pivot;      // rotationPivot, scaled
    Vector3 _artOrigin;  // art pivot at spawn
    Vector3 _artPos;     // art pivot now
    Vector2 _velocity;
    float _spin;         // degrees/sec
    float _rot;          // degrees
    float _t, _restY;
    int _bounces;

    public GameObject SpawnedCrown => _crown;
    public bool HasCrown => _crown != null;

    void Reset() => anchor = transform;

    public void SetFacing(float sign) => facing = sign < 0f ? -1f : 1f;

    // ------------------------------------------------------------------ detach

    /// <summary>UnityEvent-friendly. Hook to the sequencer's onImpact.</summary>
    public void Detach()
    {
        if (_crown != null) return;
        if (crownPrefab == null)
        {
            Debug.LogWarning("[CrownDetach] Needs a crownPrefab.", this);
            return;
        }

        if (anchor == null) anchor = transform;
        Vector3 spawn = anchor.position;
        ownerAnimator.runtimeAnimatorController = overrideController;

        // World space, never parented. Deparenting later would bake the owner's
        // lossyScale in, and a flipped combatant has scale.x = -1.
        _crown = Instantiate(crownPrefab, spawn, Quaternion.identity);
        _crownT = _crown.transform;
        _crownT.localScale = crownPrefab.transform.localScale;

        _pivot = Vector3.Scale(rotationPivot, _crownT.localScale);

        // Art pivot starts at spawn + offset, so ApplyPose at rotation 0 puts the
        // transform back exactly on the anchor and the crown lines up with the head.
        _artOrigin = spawn + _pivot;
        _artPos = _artOrigin;

        _restY = ResolveGroundY(spawn) + restOffsetY;

        if (addSilhouette) ImpactSilhouette.Attach(_crown);

        _t = 0f;
        _rot = 0f;
        _spin = 0f;
        _velocity = Vector2.zero;
        _bounces = 0;
        _phase = Phase.Arc;

        ApplyPose();
        onDetach?.Invoke();
    }

    /// <summary>UnityEvent-friendly. Hook to the sequencer's onSettle.</summary>
    public void Release()
    {
        if (_phase != Phase.Arc) return;

        _velocity = _velocity * releaseVelocityScale
                  + new Vector2(releaseVelocity.x * facing, releaseVelocity.y);
        _spin += releaseSpin * facing;

        _phase = fallOnRelease ? Phase.Fall : Phase.Rest;
    }

    /// <summary>For pooled combatants or a retry.</summary>
    public void DespawnCrown()
    {
        if (_crown == null) return;
        if (Application.isPlaying) Destroy(_crown); else DestroyImmediate(_crown);
        _crown = null;
        _crownT = null;
        _phase = Phase.Idle;
    }

    // ------------------------------------------------------------------- drive

    void Update()
    {
        if (_phase == Phase.Arc) TickArc();
        else if (_phase == Phase.Fall) TickFall();
    }

    void TickArc()
    {
        // SCALED time: the sequencer's slow motion is what suspends the crown.
        _t += Time.deltaTime;
        float k = arcDuration <= 0f ? 1f : Mathf.Clamp01(_t / arcDuration);

        _artPos = ArcPosition(k);
        _rot = ArcRotation(k);

        // Sample ahead rather than differencing frames -- during the hold
        // deltaTime is ~0.0006s and a finite difference gets noisy.
        const float look = 0.01f;
        float kAhead = arcDuration <= 0f ? 1f : Mathf.Clamp01((_t + look) / arcDuration);
        _velocity = (ArcPosition(kAhead) - _artPos) / look;
        _spin = (ArcRotation(kAhead) - _rot) / look;

        ApplyPose();
    }

    void TickFall()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        _velocity.y -= gravity * dt;
        _artPos += (Vector3)(_velocity * dt);
        _rot += _spin * dt;

        if (_artPos.y <= _restY && _velocity.y < 0f)
        {
            _artPos.y = _restY;
            _bounces++;

            _velocity.y = -_velocity.y * bounciness;
            _velocity.x *= 1f - horizontalFriction;
            _spin *= spinRetention;

            onBounce?.Invoke();

            if (_bounces >= maxBounces || _velocity.y < restThreshold)
            {
                _velocity = Vector2.zero;
                _spin = 0f;
                // Whatever angle it landed on is the resting angle. Forcing an
                // absolute target here is what made it unwind to upright.
                _phase = Phase.Rest;
                ApplyPose();
                onRest?.Invoke();
                return;
            }
        }

        ApplyPose();
    }

    // ----------------------------------------------------------------- posing

    Vector3 ArcPosition(float k)
    {
        // -lagDistance * facing: knocked right means the crown is left behind.
        return _artOrigin + new Vector3(
            arcX.Evaluate(k) * -lagDistance * facing,
            arcY.Evaluate(k) * popHeight,
            0f);
    }

    float ArcRotation(float k) => arcRotation.Evaluate(k) * tiltDegrees * facing;

    void ApplyPose()
    {
        Quaternion q = Quaternion.Euler(0f, 0f, _rot);
        _crownT.SetPositionAndRotation(_artPos - q * _pivot, q);
    }

    // ----------------------------------------------------------------- helpers

    float ResolveGroundY(Vector3 spawn)
    {
        if (groundOverride != null) return groundOverride.position.y;

        // The crown sprite matches the character's size and alignment, so its own
        // bottom edge at spawn is the character's feet -- the floor, with no floor
        // object needed. Read from sprite.bounds, not Renderer.bounds, so rotation
        // later in the fall can't corrupt it.
        var sprite = GetSprite();
        if (sprite == null) return spawn.y;
        return spawn.y + sprite.bounds.min.y * _crownT.localScale.y;
    }

    Sprite GetSprite()
    {
        var source = _crown != null ? _crown : crownPrefab;
        if (source == null) return null;
        var sr = source.GetComponentInChildren<SpriteRenderer>();
        return sr != null ? sr.sprite : null;
    }


    // ------------------------------------------------------------------ gizmos

    void OnDrawGizmosSelected()
    {
        if (anchor == null) anchor = transform;

        Vector3 scale = crownPrefab != null ? crownPrefab.transform.localScale : Vector3.one;
        Vector3 pivot = Vector3.Scale(rotationPivot, scale);
        Vector3 spawn = anchor.position;
        Vector3 origin = spawn + pivot;

        var sprite = GetSprite();
        float ground = groundOverride != null
            ? groundOverride.position.y
            : (sprite != null ? spawn.y + sprite.bounds.min.y * scale.y : spawn.y);

        // transform origin -> art pivot. The length of this line is the sideways
        // drift you'd get if the sprite rotated about its own origin.
        Gizmos.color = new Color(1f, 1f, 1f, 0.45f);
        Gizmos.DrawLine(spawn, origin);
        Gizmos.DrawWireSphere(spawn, 0.04f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(origin, 0.07f);

        // arc travelled by the art pivot
        Gizmos.color = new Color(1f, 0.55f, 1f, 0.9f);
        Vector3 prev = origin;
        for (int i = 1; i <= 24; i++)
        {
            float k = i / 24f;
            Vector3 p = origin + new Vector3(
                arcX.Evaluate(k) * -lagDistance * facing,
                arcY.Evaluate(k) * popHeight, 0f);
            Gizmos.DrawLine(prev, p);
            prev = p;
        }

        // ground line, and where the art pivot comes to rest
        Gizmos.color = new Color(0.35f, 1f, 0.55f, 0.9f);
        Gizmos.DrawLine(new Vector3(spawn.x - 2f, ground, spawn.z),
                        new Vector3(spawn.x + 2f, ground, spawn.z));
        Gizmos.DrawWireSphere(new Vector3(prev.x, ground + restOffsetY, spawn.z), 0.07f);
    }
}