using System;
using UnityEngine;

#nullable enable
namespace DialogueScripts
{
    [Serializable]
    public abstract class DialogueEvents
    {
        public abstract void Execute();
    };

    public class SetSpeaker: DialogueEvents, IEvent
    {
        public ActorProfile? actor;
        public override void Execute() => this.Invoke();
    }


    public class SpriteChange : DialogueEvents, IEvent
    {
        public ActorProfile? actor;
        public Sprite sprite = null!;
        public override void Execute() => this.Invoke();
    }

    public class ActorAction : DialogueEvents, IEvent
    {
        public ActorProfile? actor;
        public CharacterActions action = CharacterActions.SetLeft;
        public float duration = 1.0f;
        public override void Execute() => this.Invoke();
    }

    public enum CharacterActions
    {
        SetLeft = 10,
        SetMiddle = 20,
        SetRight = 30,
        SetOffscreenLeft = 40,
        SetOffscreenRight = 50,
        FadeIn = 60,
        FadeOut = 70,
        FlashFade = 80,
    }

    /// Change the vertical positioning of the dialogue box. 
    public class VerticalLayoutChange : DialogueEvents, IEvent
    {
        public Layout Layout = Layout.Lower;
        public override void Execute() => this.Invoke();
    }

    public enum Layout
    {
        Lower = 10,
        Upper = 20
    }

    /// Automatically advances this dialogue entry after it finishes. 
    public class AutoAdvanceAfter : DialogueEvents, IEvent
    {
        public float Time = 0.5f;
        public override void Execute() => this.Invoke();
    }
}
