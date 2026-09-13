using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Marks an object as part of the impact-frame silhouette. Drop this on the
/// player, the enemy, the crown, and any ParticleSystem you want stamped black.
///
/// Deliberately opt-in per object rather than layer-based: layers are shared with
/// physics in Unity, and you don't want to reshuffle collision matrices just to
/// change which things appear in a silhouette.
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("Impact/Impact Silhouette")]
public class ImpactSilhouette : MonoBehaviour
{
    [Tooltip("Renderers to stamp into the mask. Auto-filled from children if left empty.")]
    public List<Renderer> renderers = new();

    [Tooltip("Optional. Force a specific texture for alpha clipping. Leave null to " +
             "resolve from the SpriteRenderer's sprite or the material's main texture.")]
    public Texture textureOverride;

    [Tooltip("Uncheck to leave this object out of the mask without disabling the GameObject.")]
    public bool contributeToMask = true;

    static readonly List<ImpactSilhouette> s_Active = new List<ImpactSilhouette>();
    public static IReadOnlyList<ImpactSilhouette> Active => s_Active;

    void Reset()
    {
        renderers = GetComponentsInChildren<Renderer>(true).ToList();
    }

    void OnEnable()
    {
        if (renderers == null || renderers.Count() == 0)
            renderers.Add(GetComponent<Renderer>());

        if (!s_Active.Contains(this))
            s_Active.Add(this);
    }

    void OnDisable()
    {
        s_Active.Remove(this);
    }

    /// <summary>
    /// Which texture the mask shader should alpha-clip against for a given renderer.
    /// Sprites report their atlas page at runtime, which matches the baked mesh UVs.
    /// </summary>
    public Texture ResolveTexture(Renderer r)
    {
        if (textureOverride != null)
            return textureOverride;

        if (r is SpriteRenderer sr && sr.sprite != null)
            return sr.sprite.texture;

        var mat = r.sharedMaterial;
        if (mat != null && mat.mainTexture != null)
            return mat.mainTexture;

        // No texture: the whole quad becomes silhouette. Fine for solid shapes,
        // wrong for particles -- give particle materials a real texture.
        return Texture2D.whiteTexture;
    }
    public static void Attach(GameObject gameObject)
    {
        gameObject.AddComponent<ImpactSilhouette>();
    }
}
