#nullable enable
using System;
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

        [Tooltip("Default entry node for this scene, useful for testing the scene. Can be overridden in code via EntryNode.")]
        [SerializeField] private string EntryNode = "Start";

        protected virtual void Awake()
        {
            dialogueRunner.AddCommandHandler<string, float>("bg", SetBackground);
        }

        protected virtual void Start()
        {
            StorybookSceneEnum requestedEntryNode = GameStateManager.Instance.StorybookEntryNode;
            GameStateManager.Instance.StorybookEntryNode = StorybookSceneEnum.None;

            string nodeName = requestedEntryNode == StorybookSceneEnum.None
                ? EntryNode
                : requestedEntryNode.ToString();

            dialogueRunner.StartDialogue(string.IsNullOrWhiteSpace(nodeName) ? "Start" : nodeName);
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
