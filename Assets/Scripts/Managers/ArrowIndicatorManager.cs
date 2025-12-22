using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Managers {
    public class ArrowIndicatorManager : MonoBehaviour {
        [Header("References")] [SerializeField]
        private Canvas worldSpaceCanvas;

        [SerializeField] private GameObject chevronPrefab; // UI Image with chevron sprite

        [Header("Settings")] [SerializeField]
        private float chevronSpacing = 0.5f; // Distance between chevrons in world units

        [SerializeField] private float animationSpeed = 2f; // Speed of the scrolling animation
        [SerializeField] private float fadeDistance = 1f; // Distance over which chevrons fade in/out
        [SerializeField] private float padding = 0.5f; // Offset from entities before arrow starts/ends
        [SerializeField] private int poolSize = 50; // Initial pool size

        private List<GameObject> chevronPool = new List<GameObject>();
        private Dictionary<string, ArrowInstance> activeArrows = new Dictionary<string, ArrowInstance>();

        private class ArrowInstance {
            public Transform entity1;
            public Transform entity2;
            public List<GameObject> activeChevrons = new List<GameObject>();
            public float animationOffset = 0f;
        }

        void Awake() {
            if (worldSpaceCanvas == null) {
                worldSpaceCanvas = GetComponentInChildren<Canvas>();
                if (worldSpaceCanvas == null) {
                    Debug.LogError("ArrowIndicatorManager: No world space canvas found!");
                    return;
                }
            }

            InitializePool();
        }

        private void OnEnable() {
            DisplayableClass.OnHovered += HandleOnHovered;
            DisplayableClass.OnUnhovered += ClearAllArrows;
        }
        
        private void OnDisable() {
            DisplayableClass.OnHovered -= HandleOnHovered;
            DisplayableClass.OnUnhovered -= ClearAllArrows;
        }

        private void HandleOnHovered(ActionClass ac) {
            Debug.Log("ArrowIndicatorManager: HandleOnHovered");
            DrawArrow(ac.Target.transform, ac.Origin.transform);
        }

        void InitializePool() {
            if (chevronPrefab == null) {
                Debug.LogError("ArrowIndicatorManager: Chevron prefab not assigned!");
                return;
            }

            for (int i = 0; i < poolSize; i++) {
                GameObject chevron = Instantiate(chevronPrefab, worldSpaceCanvas.transform);
                chevron.SetActive(false);
                chevronPool.Add(chevron);
            }
        }

        GameObject GetChevronFromPool() {
            foreach (var chevron in chevronPool) {
                if (!chevron.activeInHierarchy) {
                    chevron.SetActive(true);
                    return chevron;
                }
            }

            // Pool exhausted, create new chevron
            GameObject newChevron = Instantiate(chevronPrefab, worldSpaceCanvas.transform);
            chevronPool.Add(newChevron);
            return newChevron;
        }

        void ReturnChevronToPool(GameObject chevron) {
            chevron.SetActive(false);
        }

        public void DrawArrow(Transform entity1, Transform entity2) {
            string key = GetArrowKey(entity1, entity2);

            if (!activeArrows.ContainsKey(key)) {
                activeArrows[key] = new ArrowInstance {
                    entity1 = entity1,
                    entity2 = entity2
                };
            }
        }

        public void RemoveArrow(Transform entity1, Transform entity2) {
            string key = GetArrowKey(entity1, entity2);

            if (activeArrows.TryGetValue(key, out ArrowInstance arrow)) {
                // Return all chevrons to pool
                foreach (var chevron in arrow.activeChevrons) {
                    ReturnChevronToPool(chevron);
                }

                arrow.activeChevrons.Clear();
                activeArrows.Remove(key);
            }
        }

        public void ClearAllArrows() {
            foreach (var arrow in activeArrows.Values) {
                foreach (var chevron in arrow.activeChevrons) {
                    ReturnChevronToPool(chevron);
                }
            }

            activeArrows.Clear();
        }

        void Update() {
            foreach (var arrow in activeArrows.Values) {
                UpdateArrow(arrow);
            }
        }

        void UpdateArrow(ArrowInstance arrow) {
            if (arrow.entity1 == null || arrow.entity2 == null) {
                return;
            }

            Vector3 actualStart = arrow.entity1.position;
            Vector3 actualEnd = arrow.entity2.position;
            Vector3 fullDirection = (actualEnd - actualStart);
            float fullDistance = fullDirection.magnitude;

            if (fullDistance < 0.1f) return; // Too close, don't draw

            fullDirection.Normalize();
            
            // Apply padding to start and end positions
            Vector3 start = actualStart + fullDirection * padding;
            Vector3 end = actualEnd - fullDirection * padding;
            float distance = (end - start).magnitude;
            
            if (distance < 0.1f) return; // After padding, too short to draw
            
            Vector3 direction = fullDirection;

            // Update animation offset
            arrow.animationOffset += animationSpeed * Time.deltaTime;
            if (arrow.animationOffset >= chevronSpacing) {
                arrow.animationOffset -= chevronSpacing;
            }

            // Calculate how many chevrons we need
            int chevronCount = Mathf.CeilToInt(distance / chevronSpacing) + 2; // +2 for fade buffer

            // Adjust active chevron count
            while (arrow.activeChevrons.Count < chevronCount) {
                arrow.activeChevrons.Add(GetChevronFromPool());
            }

            while (arrow.activeChevrons.Count > chevronCount) {
                GameObject chevron = arrow.activeChevrons[arrow.activeChevrons.Count - 1];
                arrow.activeChevrons.RemoveAt(arrow.activeChevrons.Count - 1);
                ReturnChevronToPool(chevron);
            }

            // Position and fade chevrons
            for (int i = 0; i < arrow.activeChevrons.Count; i++) {
                GameObject chevron = arrow.activeChevrons[i];
                RectTransform rectTransform = chevron.GetComponent<RectTransform>();
                Image image = chevron.GetComponent<Image>();

                // Calculate position along the line
                float t = (i * chevronSpacing - arrow.animationOffset) / distance;

                if (t < -0.1f || t > 1.1f) {
                    chevron.SetActive(false);
                    continue;
                }

                chevron.SetActive(true);
                Vector3 position = start + direction * (i * chevronSpacing - arrow.animationOffset);
                rectTransform.position = position;

                // Calculate rotation to point along the arrow
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                rectTransform.rotation = Quaternion.Euler(0, 0, angle);

                // Calculate fade based on distance from start and end
                float distFromStart = Vector3.Distance(start, position);
                float distFromEnd = Vector3.Distance(end, position);
                float fadeStart = Mathf.Clamp01(distFromStart / fadeDistance);
                float fadeEnd = Mathf.Clamp01(distFromEnd / fadeDistance);
                float alpha = Mathf.Min(fadeStart, fadeEnd);

                Color color = image.color;
                color.a = alpha;
                image.color = color;
            }
        }

        string GetArrowKey(Transform entity1, Transform entity2) {
            return entity1.GetInstanceID() + "_" + entity2.GetInstanceID();
        }
    }
}