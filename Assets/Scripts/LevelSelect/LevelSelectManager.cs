using LevelSelectInformation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static LevelSelectInformation.StageInformation;

/*
 * It seems that for the most part, the level select scene components are mostly self managed
 * e.g. Button unlock by progression, etc.
 * For other events, such as level completion triggers, they can be handled here
 */
public class LevelSelectManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup levelSelectCanvas;

    public void Start()
    {
        var showUnlockDialogue = !GameStateManager.SEASON_1_ACTIVE &&
            GameStateManager.Instance.CurrentLevelProgress >= Get<PrincessFrogFight>().LevelID &&
            GameStateManager.Instance.RecordFirstTimeEvent(OneTimeEvents.ShowPrologueGreeting);

        if (showUnlockDialogue) {
            StartCoroutine(UnlockedDialogue());
        }
    }

    IEnumerator UnlockedDialogue()
    {
        levelSelectCanvas.interactable = false;
        levelSelectCanvas.blocksRaycasts = false;
        string tutorialName = "Studio Lilac";
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(DialogueManager
            .Instance
            .StartDialogue(new List<DialogueText>
        {
            new DialogueText("Hiii, thanks for playing through Wastelanders Prologue.", tutorialName, null),
            new DialogueText("Welcome to the level select screen!", tutorialName, null),
            new DialogueText("You can select a level to replay by clicking on the buttons.", tutorialName, null),
            new DialogueText("We're hoping on finishing up Wastelanders Season 1 in the Spring so more content is coming soon!", tutorialName, null),
            new DialogueText("Get excited for a big new story, 8 new levels, and some fun unlockable enemy cards!", tutorialName, null),
            new DialogueText("Thanks for playing, and leave a review for us so we know you had fun!!! (We'll personally read every one of them out to the team.)", tutorialName, null),
            new DialogueText("Keep in touch, Studio Lilac.", tutorialName, null),
        }));
        levelSelectCanvas.interactable = true;
        levelSelectCanvas.blocksRaycasts = true;
        yield return null;
    }
}
