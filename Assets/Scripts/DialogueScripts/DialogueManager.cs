using System.Collections;
using System.Collections.Generic;
using DialogueScripts;
using UI_Toolkit;
using UnityEngine;

#nullable enable
public class DialogueManager : MonoBehaviour
{
    private const string PAGE_FLIP_SOUND_EFFECT = "Page Flip";
    public static DialogueManager Instance { get; private set; } = null!;
    private DialogueBox activeDialogueBox = null!;
    [SerializeField]
    private DialogueBox picturelessDialogueBoxComponent = null!;
    [SerializeField]
    private DialogueBox pictureDialogueBox = null!;
    [SerializeField] private Canvas dialogueCanvas = null!;
    private List<DialogueText> sentences = new();

    [SerializeField] private StageDirector stageDirectorPrefab = null!;
    [SerializeField] private Canvas stageDirectionCanvas = null!;

    private bool inDialogue = false;
    private bool wasSkipping = false;

    List<DialogueText> dialogueHistory = new();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != null && Instance != this)
        {
            Destroy(this);
        }


        Instantiate(stageDirectorPrefab, stageDirectionCanvas.transform);
        stageDirectionCanvas.sortingOrder = UISortOrder.CharacterActors.GetOrder();
        
        activeDialogueBox = pictureDialogueBox;
        ChangeDialogueBoxOrder(UISortOrder.DialogueBox.GetOrder());
    }

    //Manager is going to listen to a bunch of events that then cause it to gain new sentences and 
    void Start()
    {
        ClearPanel();
    }

    public void ChangeDialogueBoxOrder(int order)
    {
        dialogueCanvas.sortingOrder = order;
    }

    public bool IsInDialogue()
    {
        return inDialogue;
    }

    public IEnumerator StartDialogue(List<DialogueText> newSentences)
    {
        if (activeDialogueBox.gameObject.activeInHierarchy)
        {
            yield break;
        }

        activeDialogueBox.gameObject.SetActive(true);

        sentences = newSentences;
        inDialogue = true;

        DisplayNextSentence();
        yield return new WaitUntil(() => !inDialogue);
    }


    void AddLineToHistory(DialogueText text)
    {
        dialogueHistory.Add(text);
    }

    public void AddDialogueEntryToHistory(DialogueEntry entry)
    {
        var text = new DialogueText(
            entry.content, 
            entry.speaker?.characterName ?? "",
            entry.picture
        );

        dialogueHistory.Add(text);
    }

    public List<DialogueText> GetHistory()
    {
        return dialogueHistory;
    }


    void ClearPanel()
    {
        activeDialogueBox.gameObject.SetActive(false);
        sentences.Clear();
        inDialogue = false;
    }

    public void MoveBoxToBottom()
    {
        pictureDialogueBox.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0);
        pictureDialogueBox.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0);
        pictureDialogueBox.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);

        if (picturelessDialogueBoxComponent != null)
        {
            picturelessDialogueBoxComponent.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0);
            picturelessDialogueBoxComponent.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0);
            picturelessDialogueBoxComponent.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        }
    }

    public void MoveBoxToTop()
    {
        pictureDialogueBox.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 1);
        pictureDialogueBox.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1);
        pictureDialogueBox.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -1 * pictureDialogueBox.GetComponent<RectTransform>().rect.height * pictureDialogueBox.GetComponent<RectTransform>().localScale.y);

        if (picturelessDialogueBoxComponent != null)
        {
            picturelessDialogueBoxComponent.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 1);
            picturelessDialogueBoxComponent.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1);
            picturelessDialogueBoxComponent.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -1 * picturelessDialogueBoxComponent.GetComponent<RectTransform>().rect.height * picturelessDialogueBoxComponent.GetComponent<RectTransform>().localScale.y);
        }
    }

    public void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }
        DialogueText sentence = sentences[0];
        sentences.RemoveAt(0); //Uses a list instead of a Queue so we can see it in the Unity Editor

        AddLineToHistory(sentence);

        if (sentence.DisplayingImage == null && picturelessDialogueBoxComponent != null)
        {
            SetDialogueBoxActive(picturelessDialogueBoxComponent);
            picturelessDialogueBoxComponent.SetLine(sentence);

        } else
        {
            SetDialogueBoxActive(pictureDialogueBox);
            pictureDialogueBox.SetLine(sentence);
        }

        sentence.playSound();
    }

    void SetDialogueBoxActive(DialogueBox dialogueBox)
    {
        if (picturelessDialogueBoxComponent != null)
        {
            picturelessDialogueBoxComponent.gameObject.SetActive(dialogueBox == picturelessDialogueBoxComponent);
            pictureDialogueBox.gameObject.SetActive(dialogueBox == pictureDialogueBox);
            activeDialogueBox = dialogueBox;
        } else
        {
            pictureDialogueBox.gameObject.SetActive(true);
            activeDialogueBox = pictureDialogueBox;
        }
    }

    void EndDialogue()
    {
        ClearPanel();
    }

    void Update()
    {
        if (PauseMenuV2.IsPaused) return;

        HandleDialogueAdvancement();
        HandleSkipSoundPlayback();
    }

    /// Checks for player input and advances the dialogue or fast-forwards the current line.
    private void HandleDialogueAdvancement()
    {
        if (!inDialogue) return;

        bool singlePress = Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Space);
        bool holdingSkip = Input.GetKey(KeyCode.RightArrow);

        if (singlePress || holdingSkip)
        {
            if (activeDialogueBox.FinishedLine())
            {
                DisplayNextSentence();
                if (holdingSkip) wasSkipping = true;
                if (singlePress) AudioManager.Instance.PlaySFX(PAGE_FLIP_SOUND_EFFECT);
            }
            else
            {
                activeDialogueBox.StopScrollingText();
            }
        }
    }

    /// Handles playing the "Page Flip" sound at the end of a skip sequence.
    /// Sound effect should play when the key is released or the dialogue concludes.
    private void HandleSkipSoundPlayback()
    {
        bool skipKeyWasReleased = Input.GetKeyUp(KeyCode.RightArrow);
        bool dialogueHasEnded = !inDialogue;

        if (wasSkipping && (skipKeyWasReleased || dialogueHasEnded))
        {
            AudioManager.Instance.PlaySFX(PAGE_FLIP_SOUND_EFFECT);
            wasSkipping = false;
        }
    }
}
