#nullable enable
using System;
using UI_Elements.FadeScreen;
using UnityEngine;
using Yarn.Unity;

namespace Storybook
{
    /// <summary>
    /// Base class for a scene's dialogue director. Handles the shared "bg" command
    /// (crossfading to an animated background clip by key) and exposes an overrideable
    /// entry node for whichever Yarn node the scene should start on.
    /// </summary>
    public abstract class StorybookDirector : MonoBehaviour
    {
        private const float DefaultBackgroundFadeDuration = 0.5f;

        [SerializeField] protected DialogueRunner dialogueRunner = null!;
        [SerializeField] protected AnimationCrossFadeHandler bg = null!;

        [Tooltip("Default entry node for this scene. Can be overridden in code via EntryNode.")]
        [SerializeField] private string entryNode = "Start";

        /// <summary>
        /// The Yarn node this director should start dialogue on. Override this in a
        /// subclass to hardcode a node in code, or leave as-is to use the inspector value.
        /// </summary>
        protected virtual string EntryNode => entryNode;

        protected virtual void Awake()
        {
            dialogueRunner.AddCommandHandler<string, float>("bg", SetBackground);
        }

        protected virtual void Start()
        {
            dialogueRunner.StartDialogue(EntryNode);
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
