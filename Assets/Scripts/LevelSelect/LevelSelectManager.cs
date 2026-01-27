using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * It seems that for the most part, the level select scene components are mostly self managed
 * e.g. Button unlock by progression, etc.
 * For other events, such as level completion triggers, they can be handled here
 */
public class LevelSelectManager : MonoBehaviour
{

    [SerializeField] private LevelSelectButton princessFrogFightButton;
    [SerializeField] private CanvasGroup levelSelectCanvas;

    public void Start()
    {
        if (GameStateManager.Instance.FirstTimeFinished) {
            princessFrogFightButton.Lock();
            GameStateManager.Instance.FirstTimeFinished = false;
            StartCoroutine(UnlockedDialogue());
        }
    }

    IEnumerator UnlockedDialogue()
    {
        Debug.Log("Unlock Dialogue");
        levelSelectCanvas.interactable = false;
        levelSelectCanvas.blocksRaycasts = false;
        string tutorialName = "Tutorial";
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(DialogueManager
            .Instance
            .StartDialogue(new List<DialogueText>
        {
            new DialogueText("Welcome to the level select screen!", tutorialName, null),
            new DialogueText("You can select a level to play by clicking on the buttons below.", tutorialName, null),
            new DialogueText("Some levels may be locked until you complete previous levels.", tutorialName, null),
            new DialogueText("Good luck and have fun!", tutorialName, null),
        }));
        //princessFrogFightButton.Unlock(animate: true);
        levelSelectCanvas.interactable = true;
        levelSelectCanvas.blocksRaycasts = true;
        yield return null;
    }

    public void Update()
    {

    }
}
