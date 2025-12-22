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
        [SerializeField] private int poolSize = 50;

        private List<GameObject> chevronPool = new List<GameObject>();
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

            InitializePool();
        }

        private void OnEnable() {
            DisplayableClass.OnHovered += HandleOnHovered;
            DisplayableClass.OnUnhovered += ClearArrow;
        }
        
        private void OnDisable() {
            DisplayableClass.OnHovered -= HandleOnHovered;
            DisplayableClass.OnUnhovered -= ClearArrow;
        }

        private void HandleOnHovered(ActionClass ac) {
            Debug.Log("ArrowIndicatorManager: HandleOnHovered");
            
            // Check if this action is part of a clash (cause then we need to draw the clashing arrow)
            bool isClashing = false;
            if (BattleQueue.BattleQueueInstance != null) {
                foreach (var wrapper in BattleQueue.BattleQueueInstance.ProvideArray()) {
                    if (wrapper.IsClashing() && 
                        (wrapper.PlayerAction == ac || wrapper.EnemyAction == ac)) {
                        isClashing = true;
                        break;
                    }
                }
            }
            
            DrawArrow(ac.Origin.transform, ac.Target.transform, isClashing);
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
                    Image image = chevron.GetComponent<Image>();
                    if (image != null) {
                        Color color = image.color;
                        color.a = 0f;
                        image.color = color;
                    }
                    chevron.SetActive(true);
                    return chevron;
                }
            }

            GameObject newChevron = Instantiate(chevronPrefab, worldSpaceCanvas.transform);
            chevronPool.Add(newChevron);
            return newChevron;
        }

        void ReturnChevronToPool(GameObject chevron) {
            Image image = chevron.GetComponent<Image>();
            if (image != null) {
                Color color = image.color;
                color.a = 0f;
                image.color = color;
            }
            chevron.SetActive(false);
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
                ReturnChevronToPool(chevron);
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

            animationOffset += animationSpeed * Time.deltaTime;
            if (animationOffset >= chevronSpacing) {
                animationOffset -= chevronSpacing;
            }

            if (isBidirectional) {
                UpdateBidirectionalArrow(start, end, direction, distance);
            } else {
                UpdateUnidirectionalArrow(start, end, direction, distance);
            }
        }

        void UpdateUnidirectionalArrow(Vector3 start, Vector3 end, Vector3 direction, float distance) {
            int chevronCount = Mathf.CeilToInt(distance / chevronSpacing) + 2;

            while (activeChevrons.Count < chevronCount) {
                activeChevrons.Add(GetChevronFromPool());
            }

            while (activeChevrons.Count > chevronCount) {
                GameObject chevron = activeChevrons[activeChevrons.Count - 1];
                activeChevrons.RemoveAt(activeChevrons.Count - 1);
                ReturnChevronToPool(chevron);
            }

            for (int i = 0; i < activeChevrons.Count; i++) {
                GameObject chevron = activeChevrons[i];
                RectTransform rectTransform = chevron.GetComponent<RectTransform>();
                Image image = chevron.GetComponent<Image>();

                float t = (i * chevronSpacing + animationOffset) / distance;

                if (t < -0.1f || t > 1.1f) {
                    if (chevron.activeInHierarchy) {
                        Color resetColor = image.color;
                        resetColor.a = 0f;
                        image.color = resetColor;
                        chevron.SetActive(false);
                    }
                    continue;
                }

                Vector3 position = start + direction * (i * chevronSpacing + animationOffset);
                rectTransform.position = position;

                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                rectTransform.rotation = Quaternion.Euler(0, 0, angle);
                
                float distFromStart = Vector3.Distance(start, position);
                float distFromEnd = Vector3.Distance(end, position);
                float fadeStart = Mathf.Clamp01(distFromStart / fadeDistance);
                float fadeEnd = Mathf.Clamp01(distFromEnd / fadeDistance);
                float alpha = Mathf.Min(fadeStart, fadeEnd);

                Color color = image.color;
                color.a = alpha;
                image.color = color;
                
                if (!chevron.activeInHierarchy) {
                    chevron.SetActive(true);
                }
            }
        }

        void UpdateBidirectionalArrow(Vector3 start, Vector3 end, Vector3 direction, float distance) {
            Vector3 midpoint = (start + end) / 2f;
            float halfDistance = distance / 2f;
            Vector3 reverseDirection = -direction;

            int chevronsPerSide = Mathf.CeilToInt(halfDistance / chevronSpacing) + 2;
            int totalChevronCount = chevronsPerSide * 2;

            while (activeChevrons.Count < totalChevronCount) {
                activeChevrons.Add(GetChevronFromPool());
            }

            while (activeChevrons.Count > totalChevronCount) {
                GameObject chevron = activeChevrons[activeChevrons.Count - 1];
                activeChevrons.RemoveAt(activeChevrons.Count - 1);
                ReturnChevronToPool(chevron);
            }

            for (int i = 0; i < chevronsPerSide; i++) {
                GameObject chevron = activeChevrons[i];
                RectTransform rectTransform = chevron.GetComponent<RectTransform>();
                Image image = chevron.GetComponent<Image>();

                float localOffset = i * chevronSpacing + animationOffset;
                float t = localOffset / halfDistance;

                if (t < -0.1f || t > 1.1f) {
                    if (chevron.activeInHierarchy) {
                        Color resetColor = image.color;
                        resetColor.a = 0f;
                        image.color = resetColor;
                        chevron.SetActive(false);
                    }
                    continue;
                }

                Vector3 position = start + direction * localOffset;
                rectTransform.position = position;

                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                rectTransform.rotation = Quaternion.Euler(0, 0, angle);

                float distFromStart = Vector3.Distance(start, position);
                float distFromMid = Vector3.Distance(midpoint, position);
                float fadeStart = Mathf.Clamp01(distFromStart / fadeDistance);
                float fadeMid = Mathf.Clamp01(distFromMid / fadeDistance);
                float alpha = Mathf.Min(fadeStart, fadeMid);

                Color color = image.color;
                color.a = alpha;
                image.color = color;

                if (!chevron.activeInHierarchy) {
                    chevron.SetActive(true);
                }
            }

            for (int i = 0; i < chevronsPerSide; i++) {
                GameObject chevron = activeChevrons[chevronsPerSide + i];
                RectTransform rectTransform = chevron.GetComponent<RectTransform>();
                Image image = chevron.GetComponent<Image>();

                float localOffset = i * chevronSpacing + animationOffset;
                float t = localOffset / halfDistance;

                if (t < -0.1f || t > 1.1f) {
                    if (chevron.activeInHierarchy) {
                        Color resetColor = image.color;
                        resetColor.a = 0f;
                        image.color = resetColor;
                        chevron.SetActive(false);
                    }
                    continue;
                }

                Vector3 position = end + reverseDirection * localOffset;
                rectTransform.position = position;

                float angle = Mathf.Atan2(reverseDirection.y, reverseDirection.x) * Mathf.Rad2Deg;
                rectTransform.rotation = Quaternion.Euler(0, 0, angle);

                float distFromEnd = Vector3.Distance(end, position);
                float distFromMid = Vector3.Distance(midpoint, position);
                float fadeEnd = Mathf.Clamp01(distFromEnd / fadeDistance);
                float fadeMid = Mathf.Clamp01(distFromMid / fadeDistance);
                float alpha = Mathf.Min(fadeEnd, fadeMid);

                Color color = image.color;
                color.a = alpha;
                image.color = color;

                if (!chevron.activeInHierarchy) {
                    chevron.SetActive(true);
                }
            }
        }
    }
}