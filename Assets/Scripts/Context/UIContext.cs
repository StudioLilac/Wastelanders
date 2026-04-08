using System;

namespace Context {
    public enum UIContext { None, Combat, Dialogue, Custom }

    public static class UIContextManager {
        public static UIContext Current { get; private set; } = UIContext.None;
        public static UIContextCustomFlags UIContextCustomFlags { get; private set; }

        public static void Set(UIContext context, UIContextCustomFlags flags = default) {
            Current = context;
            UIContextCustomFlags = flags;
            new UIContextChangedEvent(context, flags).Invoke();
        }
    }

    [Flags] public enum UIContextCustomFlags {
        None = 0,
        AutoRoll = 1,
        DoubleSpeed = 2,
        DialogueLog = 4,
        SkipDialogue = 8,
    }

    public record UIContextChangedEvent(UIContext context, UIContextCustomFlags flags) : IEvent;
}