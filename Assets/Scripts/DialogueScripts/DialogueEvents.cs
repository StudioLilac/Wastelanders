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

    public class ExpressionChange : DialogueEvents, IEvent
    {
        public ActorProfile? actor;
        public DialogueSprite expression;
        public override void Execute() => this.Invoke();
    }

    public class ActorAction : DialogueEvents, IEvent
    {
        public ActorProfile? actor;
        public CharacterActions action = 
            CharacterActions.SetLeft;
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
        FaceRight = 90,
        FaceLeft = 100
    }

    /// Change the vertical positioning of the dialogue box. 
    public class VerticalLayoutChange : DialogueEvents, IEvent
    {
        public Layout Layout = Layout.Lower;
        public override void Execute() => this.Invoke();

        public static void MoveBoxV2ToTop() => new VerticalLayoutChange { Layout = Layout.Upper }.Invoke();
        public static void MoveBoxV2ToBottom() => new VerticalLayoutChange { Layout = Layout.Lower }.Invoke();
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
    public class CustomEvent: DialogueEvents, IEvent
    {
        public string EventName = string.Empty;
        public override void Execute() => this.Invoke();
    }

    // Runs a captured callback when the dialogue reaches this beat. Unlike CustomEvent it does
    // not go through the event bus, so a scene can close over local state (e.g. a specific
    // ControllableAudioChannel) directly instead of routing through a named string.
    public class CallbackEvent : DialogueEvents
    {
        private readonly Action callback;
        public CallbackEvent(Action callback) => this.callback = callback;
        public override void Execute() => callback?.Invoke();
    }
}
