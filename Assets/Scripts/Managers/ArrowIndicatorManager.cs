using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Managers {
    public class ArrowIndicatorManager : MonoBehaviour {
        [Header("References")]
        [SerializeField] private Canvas worldSpaceCanvas;
        [SerializeField] private GameObject chevronPrefab;

        [Header("Settings")]
        [SerializeField] private float chevronSpacing = 0.5f;
        [SerializeField] private float animationSpeed = 2f;
        [SerializeField] private float fadeDistance = 1f;
        [SerializeField] private float padding = 0.5f;

        private List<GameObject> activeChevrons = new List<GameObject>();
        
        private Transform entity1;
        private Transform entity2;
        private bool hasActiveArrow = false;
        private bool isBidirectional = false;
        private float animationOffset = 0f;

        void Awake() {
            if (worldSpaceCanvas == null) {
                worldSpaceCanvas = GetComponentInChildren<Canvas>();
                if (worldSpaceCanvas == null) {
                    Debug.LogError("ArrowIndicatorManager: No world space canvas found!");
                    return;
                }
            }

            this.Subscribe<DisplayableHoveredEvent>(HandleOnHovered);
            this.Subscribe<DisplayableUnhoveredEvent>(HandleOnUnhovered);
        }

        private void HandleOnHovered(DisplayableHoveredEvent evt) {
            var ac = evt.ActionClass;
            bool isClashing = BattleQueue.BattleQueueInstance?.IsActionPartOfClash(ac) ?? false;
            DrawArrow(ac.Origin.transform, ac.Target.transform, isClashing);
        }

        private void HandleOnUnhovered(DisplayableUnhoveredEvent evt) {
            ClearArrow();
        }

        GameObject CreateChevron() {
            GameObject chevron = Instantiate(chevronPrefab, worldSpaceCanvas.transform);
            Image image = chevron.GetComponent<Image>();
            if (image != null) {
                Color color = image.color;
                color.a = 0f;
                image.color = color;
            }
            return chevron;
        }

        void DestroyChevron(GameObject chevron) {
            Destroy(chevron);
        }

        public void DrawArrow(Transform fromEntity, Transform toEntity, bool bidirectional = false) {
            ClearArrow();
            
            entity1 = fromEntity;
            entity2 = toEntity;
            isBidirectional = bidirectional;
            hasActiveArrow = true;
            animationOffset = 0f;
        }

        public void ClearArrow() {
            foreach (var chevron in activeChevrons) {
                DestroyChevron(chevron);
            }
            activeChevrons.Clear();
            
            hasActiveArrow = false;
            entity1 = null;
            entity2 = null;
        }

        void Update() {
            if (!hasActiveArrow || entity1 == null || entity2 == null) {
                return;
            }

            Vector3 actualStart = entity1.position;
            Vector3 actualEnd = entity2.position;
            Vector3 fullDirection = (actualEnd - actualStart);
            float fullDistance = fullDirection.magnitude;

            if (fullDistance < 0.1f) return;

            fullDirection.Normalize();
            
            Vector3 start = actualStart + fullDirection * padding;
            Vector3 end = actualEnd - fullDirection * padding;
            float distance = (end - start).magnitude;
            
            if (distance < 0.1f) return;
            
            Vector3 direction = fullDirection;

            // Let animationOffset grow continuously - we'll use modulo per-chevron
            animationOffset += animationSpeed * Time.deltaTime;

            if (isBidirectional) {
                UpdateBidirectionalArrow(start, end, direction, distance);
            } else {
                UpdateUnidirectionalArrow(start, end, direction, distance);
            }
        }

        void UpdateUnidirectionalArrow(Vector3 start, Vector3 end, Vector3 direction, float distance) {
            int chevronCount = Mathf.CeilToInt(distance / chevronSpacing) + 2;

            while (activeChevrons.Count < chevronCount) {
                activeChevrons.Add(CreateChevron());
            }

            while (activeChevrons.Count > chevronCount) {
                GameObject chevron = activeChevrons[activeChevrons.Count - 1];
                activeChevrons.RemoveAt(activeChevrons.Count - 1);
                DestroyChevron(chevron);
            }

            for (int i = 0; i < activeChevrons.Count; i++) {
                GameObject chevron = activeChevrons[i];
                RectTransform rectTransform = chevron.GetComponent<RectTransform>();
                Image image = chevron.GetComponent<Image>();

                // Use modulo so each chevron wraps independently when it reaches the end
                float patternLength = activeChevrons.Count * chevronSpacing;
                float rawOffset = i * chevronSpacing + animationOffset;
                float wrappedOffset = rawOffset % patternLength;
                float t = wrappedOffset / distance;

                if (t < 0f || t > 1f) {
                    chevron.SetActive(false);
                    continue;
                }

                Vector3 position = start + direction * wrappedOffset;
                rectTransform.position = position;

                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                rectTransform.rotation = Quaternion.Euler(0, 0, angle);
                
                float distFromStart = wrappedOffset;
                float distFromEnd = distance - wrappedOffset;
                float fadeStart = Mathf.Clamp01(distFromStart / fadeDistance);
                float fadeEnd = Mathf.Clamp01(distFromEnd / fadeDistance);
                float alpha = Mathf.Min(fadeStart, fadeEnd);

                Color color = image.color;
                color.a = alpha;
                image.color = color;
                
                chevron.SetActive(true);
            }
        }

        void UpdateBidirectionalArrow(Vector3 start, Vector3 end, Vector3 direction, float distance) {
            Vector3 midpoint = (start + end) / 2f;
            float halfDistance = distance / 2f;
            Vector3 reverseDirection = -direction;

            int chevronsPerSide = Mathf.CeilToInt(halfDistance / chevronSpacing) + 2;
            int totalChevronCount = chevronsPerSide * 2;

            while (activeChevrons.Count < totalChevronCount) {
                activeChevrons.Add(CreateChevron());
            }

            while (activeChevrons.Count > totalChevronCount) {
                GameObject chevron = activeChevrons[activeChevrons.Count - 1];
                activeChevrons.RemoveAt(activeChevrons.Count - 1);
                DestroyChevron(chevron);
            }

            for (int i = 0; i < chevronsPerSide; i++) {
                GameObject chevron = activeChevrons[i];
                RectTransform rectTransform = chevron.GetComponent<RectTransform>();
                Image image = chevron.GetComponent<Image>();

                // Use modulo so each chevron wraps independently
                float patternLength = chevronsPerSide * chevronSpacing;
                float rawOffset = i * chevronSpacing + animationOffset;
                float localOffset = rawOffset % patternLength;
                float t = localOffset / halfDistance;

                if (t < 0f || t > 1f) {
                    chevron.SetActive(false);
                    continue;
                }

                Vector3 position = start + direction * localOffset;
                rectTransform.position = position;

                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                rectTransform.rotation = Quaternion.Euler(0, 0, angle);

                float distFromStart = localOffset;
                float distFromMid = halfDistance - localOffset;
                float fadeStart = Mathf.Clamp01(distFromStart / fadeDistance);
                float fadeMid = Mathf.Clamp01(distFromMid / fadeDistance);
                float alpha = Mathf.Min(fadeStart, fadeMid);

                Color color = image.color;
                color.a = alpha;
                image.color = color;

                chevron.SetActive(true);
            }

            for (int i = 0; i < chevronsPerSide; i++) {
                GameObject chevron = activeChevrons[chevronsPerSide + i];
                RectTransform rectTransform = chevron.GetComponent<RectTransform>();
                Image image = chevron.GetComponent<Image>();

                // Use modulo so each chevron wraps independently
                float patternLength = chevronsPerSide * chevronSpacing;
                float rawOffset = i * chevronSpacing + animationOffset;
                float localOffset = rawOffset % patternLength;
                float t = localOffset / halfDistance;

                if (t < 0f || t > 1f) {
                    chevron.SetActive(false);
                    continue;
                }

                Vector3 position = end + reverseDirection * localOffset;
                rectTransform.position = position;

                float angle = Mathf.Atan2(reverseDirection.y, reverseDirection.x) * Mathf.Rad2Deg;
                rectTransform.rotation = Quaternion.Euler(0, 0, angle);

                float distFromEnd = localOffset;
                float distFromMid = halfDistance - localOffset;
                float fadeEnd = Mathf.Clamp01(distFromEnd / fadeDistance);
                float fadeMid = Mathf.Clamp01(distFromMid / fadeDistance);
                float alpha = Mathf.Min(fadeEnd, fadeMid);

                Color color = image.color;
                color.a = alpha;
                image.color = color;

                chevron.SetActive(true);
            }
        }
    }
}