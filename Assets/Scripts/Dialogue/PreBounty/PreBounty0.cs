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
        private const float PAN_DURATION = 2.5f;
        [SerializeField] private UIFadeHandler fader;
        [SerializeField] private CanvasGroupFadeHandler comingSoon;

        [SerializeField] private UIFadeHandler wastelandBackground;

        private int keyframeIndex;
        private Coroutine panCoroutine;
        
        private IEnumerator Start()
        {
            this.Subscribe<CustomEvent>(HandleDialogueEvent);
            keyframeIndex = 0;
            comingSoon.SetLightScreen();
            fader.SetDarkScreen();
            yield return DialogueBoxV2.Instance.Play(Scene0Dialogue.ReadyOrNot);
            HandleCameraPanEvent();
            yield return new WaitForSeconds(1.0f);
            StartCoroutine(fader.FadeInLightScreen(2.0f));
            yield return new WaitForSeconds(4.0f);
            HandleCameraPanEvent();
            yield return new WaitForSeconds(3f);
            HandleCameraPanEvent();
            yield return new WaitForSeconds(3f);
            yield return DialogueBoxV2.Instance.Play(Scene0Dialogue.OpenerDialogue(this));
            yield return new WaitForSeconds(1.0f);

            GameStateManager.Instance.LoadScene(SceneData.Get<SceneData.MainMenu>().SceneName, shouldFade: false);
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
            StartCoroutine(wastelandBackground.FadeInDarkScreen(0.5f));
        }

        public void HandleCameraPanEvent()
        {
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

            while (elapsed < PAN_DURATION)
            {
                float t = elapsed / PAN_DURATION;
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

    public static class Scene0Dialogue
    {
        public static DialogueAsCode ReadyOrNot => new DialogueAsCode()
            .Line(DialogueCharacter.Ailin, "Ready or not, here I come!");

        public static DialogueAsCode OpenerDialogue(PreBounty0 bounty) => new DialogueAsCode()
            .Line(DialogueCharacter.Ailin, "Now who could that be hiding in this alleyway...?")
            .Line(DialogueCharacter.Ailin, "Found you!")
            .Line(DialogueCharacter.Jackie, "What! I thought I had you this time. How'd you know?")
            .Line(DialogueCharacter.Ailin, "Take a look, dear. Your shadow is showing.")
            .Line(DialogueCharacter.Jackie, "Ugh, what a dumb mistake.")
            .Line(DialogueCharacter.Ailin, "That's alright dear, now you'll be more aware the next time.")
            .Line(DialogueCharacter.Jackie, "Then one more time?")
            .Line(DialogueCharacter.Ailin, "Sorry dear, that's it for today.")
            .Line(DialogueCharacter.Ailin, "A business trip came up. I have to prepare to head out tomorrow.")
            .Line(DialogueCharacter.Jackie, "But Ma... you just got back.")
            .Do(new CustomEvent())
            .Line(DialogueCharacter.Ailin, "I know, sweetheart, but it’s for your Uncle Jay. He’s gone missing in the Tundra.")
            .Line(DialogueCharacter.Jackie, "No... Will he be okay?")
            .Line(DialogueCharacter.Ailin, "Yes. He’s the hardiest scout I know. He’ll be alright.")
            .Line(DialogueCharacter.Jackie, "And you’re the hardiest Ma I know!")
            .Line(DialogueCharacter.Jackie, "Promise me you’ll be alright too?")
            .Line(DialogueCharacter.Ailin, "Haha, of course, Jackie. I’ll be back soon, I promise.")
            .Line(DialogueCharacter.Ailin, "I trust you'll be good for Aunt Ives, too?")
            .Line(DialogueCharacter.Jackie, "I promise!")
            .Line(DialogueCharacter.Ailin, "Haha, it's a deal then.")
            .Line(DialogueCharacter.Narration, "<i>The mother and daughter embrace, letting goodbyes stay unspoken.");
    }
}
