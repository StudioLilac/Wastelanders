#nullable enable
using System;
using System.Collections;
using UI_Elements.FadeScreen;
using UnityEngine;
using Yarn.Unity;

namespace Storybook
{
    public enum StorybookSceneEnum {
        None,
        Storybook1,
        Storybook2,
        Storybook3,
        Storybook4
    }

    [Serializable]
    public struct StoryBookAudio
    {
        public AudioClip? Prologue1;
        public AudioClip? Prologue2;
        public AudioClip? Prologue3;
    }

    public static class StorybookSceneEnumExtensions
    {
        public static string ToNodeName(this StorybookSceneEnum scene)
        {
            return scene == StorybookSceneEnum.None ? "Start" : scene.ToString();
        }

        public static AudioClip? GetMusic(this StorybookSceneEnum scene, StoryBookAudio audio)
        {
            return scene switch
            {
                StorybookSceneEnum.Storybook1 => null, // Let the audio flow through from the prologue.
                StorybookSceneEnum.Storybook2 => audio.Prologue2,
                StorybookSceneEnum.Storybook3 => audio.Prologue2,
                StorybookSceneEnum.Storybook4 => audio.Prologue3,
                _ => audio.Prologue1
            };
        }
    }


    /// <summary>
    /// Base class for a scene's dialogue director. Handles the shared "bg" command
    /// (crossfading to an animated background clip by key) and exposes an overrideable
    /// entry node for whichever Yarn node the scene should start on.
    /// </summary>
    public class StorybookDirector : MonoBehaviour
    {
        private const float DefaultBackgroundFadeDuration = 0.5f;

        [SerializeField] protected DialogueRunner dialogueRunner = null!;
        [SerializeField] protected AnimationCrossFadeHandler bg = null!;
        [SerializeField] protected UIFadeHandler uiFadeHandler = null!;

        [Tooltip("Default entry node for this scene, useful for testing the scene. Can be overridden in code via EntryNode.")]
        [SerializeField] private StoryBookAudio storybookAudio = default!;

        protected virtual void Awake()
        {
            dialogueRunner.AddCommandHandler<string, float>("bg", SetBackground);
            dialogueRunner.onDialogueComplete?.AddListener(OnDialogueComplete);
        }

        protected virtual void OnDestroy()
        {
            dialogueRunner.onDialogueComplete?.RemoveListener(OnDialogueComplete);
        }

        protected virtual void Start()
        {
            uiFadeHandler.SetLightScreen();
            StorybookSceneEnum requestedEntryNode = GameStateManager.Instance.Payload<StoryBookEntry>()?.StorybookEntryNode ?? StorybookSceneEnum.None;
            AudioClip? audio = requestedEntryNode.GetMusic(storybookAudio);
            if (audio != null)
            {
                AudioManager.Instance.FadeInBackgroundTrack(1f, audio, true);
            }
            dialogueRunner.StartDialogue(requestedEntryNode.ToNodeName());
        }
        
        protected virtual void OnDialogueComplete() {
            StartCoroutine(End());
        }

        private IEnumerator End() {
            yield return StartCoroutine(uiFadeHandler.FadeInDarkScreen(1f));
            GameStateManager.Instance.LoadScene(SceneData.Get<SceneData.StorybookSelector>().SceneName);
        }

        private static string ToNodeName(StorybookSceneEnum scene)
        {
            return scene == StorybookSceneEnum.None ? "Start" : scene.ToString();
        }

        private void SetBackground(string key, float duration = DefaultBackgroundFadeDuration)
        {
            if (!StorybookBackgroundLibrary.Shared.TryGet(key, out var clip))
            {
                throw new Exception($"unknown background key: {key}");
            }

            bg.CrossFadeTo(clip, duration);
        }
    }
}
