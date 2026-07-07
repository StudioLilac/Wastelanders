using DialogueScripts;
using UnityEngine;
using static DialogueCharacter;

namespace Tutorials
{
    public static class EnemyActionTutorial
    {
        public static DialogueAsCode Explanation => new DialogueAsCode()
            .Line(Tutorial, "New weapon deck unlocked: RIP Gloves.")
            .Do(new CustomEvent {  EventName = DeckSelectionTutorial.DismissInteractionBlocker })
            .Line(Tutorial, "Princess Frog Actions can now be played.")
            .Line(Tutorial, "Press 'Edit deck' to view and customize your new Actions.");

    }
}
