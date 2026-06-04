/*
 * Enum that decides the sorting order of all UI elements.
 * Feel free to add extra items to this, everything will automatically sort itself. 
 */
public enum UISortOrder
{
    Base,
    Hudv2,
    CharacterActors,
    DialogueBox,
    WarningPopup,
    CombatIntro,
    GameOverScrim,
    GameOverText,
    GameOverDialogue,
    PauseMenu,
    FadeScreen,
    SaveIndicator
}

public static class UiSortOrderHelpers
{
    public static int GetOrder(this UISortOrder order) => (int) order;
} 