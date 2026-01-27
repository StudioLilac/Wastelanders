using DialogueScripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dialogue.PreBounty {
    public class PreBounty0 : MonoBehaviour {
        [SerializeField] private DialogueWrapper ailinDialogue;
        [SerializeField] private new Camera camera;
        [SerializeField] private List<Transform> keyframes;
        [SerializeField] private List<float> zoomValues;
        [SerializeField] private float panDuration = 1.5f;
        [SerializeField] private UIFadeHandler fader;
        [SerializeField] private CanvasGroupFadeHandler comingSoon;

        private int keyframeIndex;
        private Coroutine panCoroutine;
        
        private IEnumerator Start()
        {
            this.Subscribe<CustomEvent>(HandleDialogueEvent);
            keyframeIndex = 0;
            comingSoon.SetLightScreen();
            fader.SetDarkScreen();
            yield return new WaitForSeconds(1.0f);
            StartCoroutine(fader.FadeInLightScreen(2.0f));
            yield return new WaitForSeconds(1.0f);
            yield return DialogueBoxV2.Instance.Play(ailinDialogue);
            yield return fader.FadeInDarkScreen(1f);
            yield return new WaitForSeconds(1.0f);
            yield return comingSoon.FadeInDarkScreen(1f);
            yield return new WaitForSeconds(4f);
            yield return comingSoon.FadeInLightScreen(1f);
            GameStateManager.Instance.LoadScene(SceneData.Get<SceneData.MainMenu>().SceneName);
        }


        private float cycleScaling = 2f;
        private float bobbingAmount = 4f; 
        private float timer = 0;
        private float verticalOffset = 0;

        void Update()
        {
            float previousOffset = verticalOffset;
            float waveslice = Mathf.Sin(cycleScaling * timer);
            timer += Time.deltaTime;
            if (timer > Mathf.PI * 2)
            {
                timer = timer - (Mathf.PI * 2);
            }

            verticalOffset = waveslice * bobbingAmount;
            float translateChange = verticalOffset - previousOffset;
            comingSoon.transform.position = new Vector3(comingSoon.transform.position.x,
            comingSoon.transform.position.y + translateChange, comingSoon.transform.position.z);
        }

        private void HandleDialogueEvent(CustomEvent evt) {
            if (keyframeIndex >= keyframes.Count)
                return;

            if (panCoroutine != null)
                StopCoroutine(panCoroutine);
            
            panCoroutine = StartCoroutine(PanCameraToKeyframe(keyframes[keyframeIndex], zoomValues[keyframeIndex]));
            keyframeIndex++;
        }

        private IEnumerator PanCameraToKeyframe(Transform keyframe, float targetZoom)
        {
            Vector3 startPos = camera.transform.position;
            Quaternion startRot = camera.transform.rotation;
            float startZoom = camera.orthographicSize;

            Vector3 endPos = keyframe.position;
            Quaternion endRot = keyframe.rotation;

            float elapsed = 0f;

            while (elapsed < panDuration)
            {
                float t = elapsed / panDuration;
                t = EaseInOutQuad(t); // quadratic easing

                camera.transform.position = Vector3.Lerp(startPos, endPos, t);
                camera.transform.rotation = Quaternion.Slerp(startRot, endRot, t);
                camera.orthographicSize = Mathf.Lerp(startZoom, targetZoom, t);

                elapsed += Time.deltaTime;
                yield return null;
            }

            // Snap to final position
            camera.transform.position = endPos;
            camera.transform.rotation = endRot;
            camera.fieldOfView = targetZoom;

            panCoroutine = null;
        }

        // --- Quadratic easing helpers ---
        private float EaseInOutQuad(float t)
        {
            return t < 0.5f
                ? 2f * t * t
                : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
        }
    }
}
