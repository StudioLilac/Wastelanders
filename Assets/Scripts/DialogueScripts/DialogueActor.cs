using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace DialogueScripts
{
    public class DialogueActor : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Image bodyRenderer;
        [SerializeField] private UIFadeHandler fadeHandler;

        [Header("Movement Settings")]
        [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);


#nullable enable
        private Coroutine? activeMoveRoutine;

        public DialogueActor ChangeSprite(Sprite newSprite)
        {
            if (bodyRenderer != null) bodyRenderer.sprite = newSprite;
            return this;
        }

        public void SetSpeaker(bool isSpeaking)
        {
            Color dimColor = new(0.6f, 0.6f, 0.6f, bodyRenderer.color.a);
            Color activeColor = new(1, 1, 1, bodyRenderer.color.a);
            bodyRenderer.color = isSpeaking ? activeColor : dimColor;

            float targetScale = isSpeaking ? 1.02f : 1.0f;
            transform.localScale = Vector3.one * targetScale;
        }


        public DialogueActor FadeActor(bool fadeIn, float duration)
        {
            if (duration == 0)
            {
                if (fadeIn)
                    fadeHandler.SetDarkScreen();
                else
                    fadeHandler.SetLightScreen();
            }
            else
            {
                StartCoroutine(fadeIn ? fadeHandler.FadeInDarkScreen(duration) : fadeHandler.FadeInLightScreen(duration));
            }

            return this;
        }

        public DialogueActor FlashActor(float duration)
        {
            StartCoroutine(FlashActorRoutine(duration));
            return this;
        }

        private IEnumerator FlashActorRoutine(float duration)
        {
            yield return StartCoroutine(fadeHandler.FadeInDarkScreen(duration / 2));
            yield return StartCoroutine(fadeHandler.FadeInLightScreen(duration / 2));
        }

        public DialogueActor MoveTo(Vector3 targetWorldPosition, float duration)
        {
            if (targetWorldPosition.x > transform.position.x)
                FaceRight();
            else if (targetWorldPosition.x < transform.position.x)
                FaceLeft();

            if (activeMoveRoutine != null) StopCoroutine(activeMoveRoutine);

            if (duration == 0)
                transform.position = targetWorldPosition;
            else
                activeMoveRoutine = StartCoroutine(MoveRoutine(targetWorldPosition, duration));
            
            return this;
        }

        private IEnumerator MoveRoutine(Vector3 targetPos, float duration)
        {
            Vector3 startPos = transform.position;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = moveCurve.Evaluate(elapsed / duration);

                transform.position = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }

            transform.position = targetPos;
            activeMoveRoutine = null;
        }

        public void FaceLeft()
        {
            bodyRenderer.transform.localScale = new Vector3(1, 1, 1);
        }

        public void FaceRight()
        {
            bodyRenderer.transform.localScale = new Vector3(-1, 1, 1);
        }
    }
}