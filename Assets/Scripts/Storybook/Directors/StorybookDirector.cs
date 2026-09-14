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
        [SerializeField] protected DialogueRunner dialogueRunner = null!;
        [SerializeField] protected AnimationCrossFadeHandler bg = null!;

        [Tooltip("Backgrounds are addressed from Yarn using 1-based keys, e.g. <<bg 1>> selects backgrounds[0].")]
        [SerializeField] private AnimationClip[] backgrounds = Array.Empty<AnimationClip>();

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

        private void SetBackground(string key, float duration = 0f)
        {
            if (!int.TryParse(key, out var index) || index < 1 || index > backgrounds.Length)
            {
                throw new Exception($"unknown background key: {key}");
            }

            bg.CrossFadeTo(backgrounds[index - 1], duration);
        }
    }
}