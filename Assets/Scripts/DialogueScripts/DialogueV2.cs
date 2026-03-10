using System;
using System.Collections.Generic;
using System.Linq;
using Steamworks;
using UnityEngine;

namespace DialogueScripts
{
    [CreateAssetMenu(menuName = "Dialogue/DialogueEntryWrapper")]
    public class DialogueEntryWrapper : ScriptableObject
    {
        public DialogueEntryInUnityEditor[] Entries;

        public static implicit operator DialogueEntry[](DialogueEntryWrapper wrapper)
        {
            return wrapper.Entries.Into();
        }
    }

#nullable enable
    [Serializable]
    public class DialogueEntryInUnityEditor
    {
        [SerializeField] [TextArea(1, 5)] private string content = null!;
        [SerializeField] internal ActorProfile speaker = null!;
        [SerializeField] private SoundID sfxId = default!;
        [SerializeField] private Sprite picture = null!;
        [SerializeReference] internal List<DialogueEvents> events = null!;

        public DialogueEntry Into() => new(
            content: content,
            speaker: speaker,
            sfxId: sfxId,
            picture: picture,
            events: events
        );
    }

    public record DialogueEntry(
        string content,
        ActorProfile? speaker,
        SoundID sfxId,
        Sprite? picture,
        List<DialogueEvents> events
        );

    public static class DialogueExtensions
    {
        public static DialogueEntry[] Into(this DialogueEntryInUnityEditor[] entries)
        {
            return Array.ConvertAll(entries, e => e.Into());
        }

        public static DialogueEntry[] Into(this List<DialogueText> wrapper)
        {
            return wrapper.Select(w => w.Into()).ToArray();
        }
    }
}