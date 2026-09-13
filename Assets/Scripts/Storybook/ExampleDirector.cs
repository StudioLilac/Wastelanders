#nullable enable

using System;
using UnityEngine;
using Yarn.Unity;

namespace Storybook
{
    public class ExampleDirector : MonoBehaviour
    {
        [SerializeField] private DialogueRunner dialogueRunner = null!;
        [SerializeField] private CrossFadeHandler bg = null!;

        [SerializeField] private Sprite? sprite1;
        [SerializeField] private Sprite? sprite2;

        private void Awake()
        {
            dialogueRunner.AddCommandHandler<string, float>("bg", SetBackground);
        }

        private void SetBackground(string key, float duration = 0f)
        {
            var sprite = key switch
            {
                "1" => sprite1,
                "2" => sprite2,
                _ => throw new Exception($"unknown key: {key}")
            };

            bg.CrossFadeTo(sprite, duration);
        }
    }
}