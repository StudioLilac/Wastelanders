using System;

namespace Context {
    public abstract record UIContext()
    {
        public record None : UIContext;
        public record Combat : UIContext;
        public record Dialogue : UIContext;
        public record Custom(UIContextCustomFlags Flags) : UIContext;
    }

    public static class UIContextManager {
        public static UIContext Current { get; private set; } = new UIContext.None();

        public static void Set(UIContext context) {
            Current = context;
            new UIContextChangedEvent(context).Invoke();
        }
    }

    [Flags] public enum UIContextCustomFlags {
        None = 0,
        AutoRoll = 1,
        DoubleSpeed = 2,
        DialogueLog = 4,
        SkipDialogue = 8,
    }

    public record UIContextChangedEvent(UIContext context) : IEvent;
}