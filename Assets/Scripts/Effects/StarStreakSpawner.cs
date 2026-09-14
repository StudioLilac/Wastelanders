using System.Collections.Generic;
using UnityEngine;

namespace Cinematics
{
    /// <summary>
    /// Spawns a streak quad behind every star sprite under <see cref="starLayerRoot"/>.
    ///
    /// A one or two pixel star has nowhere to draw a spike, same problem the moon
    /// had: the sprite's quad is the canvas. Rather than hand-placing a blot on each
    /// one, this walks the layer at startup and parents a correctly sized quad to
    /// each star.
    ///
    /// The quads are a fixed world size regardless of how big the star is. Spike
    /// length is a property of the eye, not of the light, so a two pixel star and a
    /// one pixel star streak the same distance.
    /// </summary>
    public class StarStreakSpawner : MonoBehaviour
    {
        [SerializeField] private Transform starLayerRoot;
        [SerializeField] private Sprite squareSprite;
        [SerializeField] private Material streakMaterial;

        [Tooltip("World units across each streak quad. Independent of star size.")]
        [SerializeField] private float quadWorldSize = 0.6f;

        [Tooltip("Sorting order offset relative to the star it sits behind.")]
        [SerializeField] private int sortingOffset = -1;

        [Tooltip("Skip stars further than this from the moon. 0 disables the cull.")]
        [SerializeField] private float maxDistanceFromMoon = 0f;

        [SerializeField] private Transform moon;

        [SerializeField] private bool spawnOnAwake = true;

        private readonly List<GameObject> spawned = new List<GameObject>();

        private void Awake()
        {
            if (spawnOnAwake) Spawn();
        }

        public void Spawn()
        {
            Clear();

            if (starLayerRoot == null || squareSprite == null || streakMaterial == null)
            {
                Debug.LogError("[StarStreakSpawner] Missing star root, square sprite or streak material.");
                return;
            }

            SpriteRenderer[] stars = starLayerRoot.GetComponentsInChildren<SpriteRenderer>(false);

            foreach (SpriteRenderer star in stars)
            {
                if (star.sharedMaterial == streakMaterial) continue;

                if (maxDistanceFromMoon > 0f && moon != null &&
                    Vector2.Distance(star.transform.position, moon.position) > maxDistanceFromMoon)
                    continue;

                var go = new GameObject($"{star.name}_Streak");
                go.transform.SetParent(star.transform, false);
                go.transform.localPosition = Vector3.zero;

                // Undo the star's own scale so every quad ends up the same world size.
                Vector3 parentScale = star.transform.lossyScale;
                go.transform.localScale = new Vector3(
                    quadWorldSize / Mathf.Max(Mathf.Abs(parentScale.x), 1e-4f),
                    quadWorldSize / Mathf.Max(Mathf.Abs(parentScale.y), 1e-4f),
                    1f);

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = squareSprite;
                sr.sharedMaterial = streakMaterial;
                sr.sortingLayerID = star.sortingLayerID;
                sr.sortingOrder = star.sortingOrder + sortingOffset;

                spawned.Add(go);
            }

            Debug.Log($"[StarStreakSpawner] Spawned {spawned.Count} streak quads.");
        }

        public void Clear()
        {
            foreach (GameObject go in spawned)
            {
                if (go == null) continue;
                if (Application.isPlaying) Destroy(go);
                else DestroyImmediate(go);
            }
            spawned.Clear();
        }
    }
}
