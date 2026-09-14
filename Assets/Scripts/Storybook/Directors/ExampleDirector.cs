#nullable enable

namespace Storybook
{
    // With everything shared handled by the base class, a concrete scene director
    // can be this simple — inspector fields (dialogueRunner, bg, backgrounds, entryNode)
    // are inherited and set per-scene in the Unity Editor.
    public class ExampleDirector : StorybookDirector
    {
    }

    // If a scene needs a different / hardcoded entry node instead of the inspector
    // field, override the property:
    //
    // public class IntroDirector : StorybookDirector
    // {
    //     protected override string EntryNode => "IntroStart";
    // }
}