#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Storybook
{
    [CreateAssetMenu(menuName = "Storybook/Background Library")]
    public sealed class StorybookBackgroundLibrary : ScriptableObject
    {
        private const string ResourcePath = "Storybook/StorybookBackgroundLibrary";

        [Serializable]
        public sealed class Entry
        {
            public string key = "";
            public AnimationClip clip = null!;
        }

        [SerializeField] private Entry[] backgrounds = Array.Empty<Entry>();

        private Dictionary<string, AnimationClip>? clipsByKey;
        private static StorybookBackgroundLibrary? shared;

        public static StorybookBackgroundLibrary Shared => shared ??= Resources.Load<StorybookBackgroundLibrary>(ResourcePath)
            ?? throw new InvalidOperationException($"Missing Resources/{ResourcePath} asset.");

        public bool TryGet(string key, out AnimationClip clip)
        {
            clipsByKey ??= CreateLookup();
            return clipsByKey.TryGetValue(key, out clip!);
        }

        private Dictionary<string, AnimationClip> CreateLookup()
        {
            var lookup = new Dictionary<string, AnimationClip>(StringComparer.OrdinalIgnoreCase);
            foreach (var background in backgrounds)
            {
                if (!string.IsNullOrWhiteSpace(background.key) && background.clip != null)
                {
                    lookup.Add(background.key, background.clip);
                }
            }

            return lookup;
        }

        private void OnValidate()
        {
            clipsByKey = null;
        }
    }
}
